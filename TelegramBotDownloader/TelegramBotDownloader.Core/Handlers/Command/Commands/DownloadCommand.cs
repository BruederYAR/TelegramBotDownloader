using Fclp;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotDownloader.Core.Attributes;
using TelegramBotDownloader.Core.Entities;
using TelegramBotDownloader.Core.Entities.Dto;
using TelegramBotDownloader.Core.Handlers.Download;

namespace TelegramBotDownloader.Core.Handlers.Command.Commands
{
    [Command("/download")]
    internal class DownloadCommand : ICommandHandler, ICommandCallBack
    {
        private readonly ILogger<DownloadCommand> _logger;
        private readonly Options _options;
        private readonly DownloadFabricManager _downloadFabricManager;

        private readonly List<string> _downloadMethodNames = new();
        private List<int> _deleteMessages = new();

        public DownloadCommand(ILogger<DownloadCommand> logger, Options options, DownloadFabricManager downloadFabricManager)
        {
            _logger = logger;
            _options = options;
            _downloadFabricManager = downloadFabricManager;

            var typesDownloadMethod = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.BaseType == typeof(DownloadFabric));

            foreach(var type in typesDownloadMethod)
            {
                var textCommandAttribute = type.GetCustomAttribute<DownloadMethodAttribute>();
                if (textCommandAttribute == null)
                    continue;

                _downloadMethodNames.Add(textCommandAttribute.Name.ToLower());
            }
        }

        public async Task HandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, PostPool posts, UserCommand command)
        {
            //if post not found
            if (posts.Messages is null || posts.Messages.Count == 0)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Post not found");
                return;
            }
            //if the command is not for this class
            if (command is null || command?.Name != this.GetType().GetCustomAttribute<CommandAttribute>()?.CommandName)
            {
                return;
            }
            //if download method is correct 
            if (command?.Args.Count < 1 || !_downloadMethodNames.Contains(command?.Args[0].ToLower()))
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id ,$"Enter the correct download method");
                return;
            }

            var dto = await ParserCmdArguments(botClient, update, command);

            var standartDir = _options.DownloadFolder + Path.DirectorySeparatorChar + dto.DownloadMethod;
            if (!Directory.Exists(standartDir))
            {
                Directory.CreateDirectory(standartDir);
            }

            if(dto.Directory is null && !dto.IsStandartDirectory)
            {
                await RequestDirectory(botClient, update, command, dto);
            }
            else
            {
                await DownloadGivenDirectory(botClient, update, cancellationToken, posts, dto);
            }
        }

        private async Task DownloadGivenDirectory(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, PostPool posts, DownloadCommandDto dto)
        {
            //Directrory and Standart Directrory in same line
            if (dto.Directory is not null && dto.IsStandartDirectory) 
            {
                var errorString = "the arguments \"standart - dir\" and \"dir\" cannot be on the same line";
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, errorString);
                throw new ArgumentException(errorString);
            }

            string standartDirectory = _options.DownloadFolder + Path.DirectorySeparatorChar + dto.DownloadMethod;
            string? downloadDirectory = null;

            if (dto.IsStandartDirectory)
            {
                downloadDirectory = standartDirectory;
            }

            if(dto.Directory is not null)
            {
                var localPath = standartDirectory + dto.Directory;
                if (!Directory.Exists(localPath) && !dto.IsMakeDirectory)
                {
                    var errorString = $"Directory {localPath} not found";
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, errorString);
                    throw new ArgumentException(errorString);
                }
                if (!Directory.Exists(localPath) && dto.IsMakeDirectory)
                {
                    Directory.CreateDirectory(localPath);
                }
                downloadDirectory = localPath;
            }

            if (downloadDirectory is null)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Directory not found");
                return;
            }

            var downloadMethod = _downloadFabricManager.GetDownloadMethod(dto.DownloadMethod, downloadDirectory);
            await DownloadAndDelete(botClient, update, cancellationToken, downloadMethod, posts);
        }

        private async Task RequestDirectory(ITelegramBotClient botClient, Update update, UserCommand command, DownloadCommandDto dto)
        {
            Menu dirMenu = new Menu(botClient, update.Message.Chat.Id, "Change directory");

            var callbackStandart = new UserCommand() { Name = command.Name, Args = new List<string>() };
            callbackStandart.Args.Add(dto.DownloadMethod);
            callbackStandart.Args.Add("--dir");
            callbackStandart.Args.Add("/");
            dirMenu.AddButton("/", JsonSerializer.Serialize<UserCommand>(callbackStandart));

            foreach (var dir in GetDirectoryes(_options.DownloadFolder + Path.DirectorySeparatorChar + dto.DownloadMethod))
            {
                var callback = new UserCommand() { Name = command.Name, Args = new List<string>() };
                callback.Args.Add(dto.DownloadMethod);
                callback.Args.Add("--dir");
                callback.Args.Add(dir);
                dirMenu.AddButton(dir, JsonSerializer.Serialize<UserCommand>(callback));
            }
            int id = await dirMenu.Show();
            _deleteMessages.Add(id);
        }

        public async Task HandleCallBackAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, PostPool posts, UserCommand commands)
        {
            try
            {
                _deleteMessages.ForEach(async (message) => await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, message));
                _deleteMessages.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            if (update.CallbackQuery is null || string.IsNullOrEmpty(update.CallbackQuery.Data))
            {
                throw new ArgumentNullException(nameof(update.CallbackQuery));
            }
            var callback = update.CallbackQuery.Data;

            var callbackCommand = JsonSerializer.Deserialize<UserCommand>(callback);
            if (callbackCommand is null)
            {
                return;
            }

            var dto = await ParserCmdArguments(botClient, update, callbackCommand);
            

            if (!_downloadMethodNames.Contains(dto.DownloadMethod))
            {
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Enter the correct download method");
                return;
            }

            var downloadMethod = _downloadFabricManager.GetDownloadMethod(dto.DownloadMethod, _options.DownloadFolder + Path.DirectorySeparatorChar + dto.DownloadMethod + Path.DirectorySeparatorChar + dto.Directory);
            await DownloadAndDelete(botClient, update, cancellationToken, downloadMethod, posts);
        }

        private async Task DownloadAndDelete(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, DownloadFabric downloadMethod, PostPool post)
        {
            var chatid = update.Message is null ? update.CallbackQuery.Message.Chat.Id : update.Message.Chat.Id;

            try
            {
                await downloadMethod.DownloadAsync(botClient, cancellationToken, post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            foreach (var message in post.Messages)
            {
                try
                {
                    await botClient.DeleteMessageAsync(chatid, message.MessageId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            post.Messages.Clear();
        }

        private List<string> GetDirectoryes(string directory)
        {    
            var directoryes = Directory.GetDirectories(directory, "*", SearchOption.AllDirectories).ToList();
            directoryes.RemoveAll(o => o.Contains("Post_"));

            for(int i = 0; i < directoryes.Count; i++)
            {
                directoryes[i] = directoryes[i].Replace(directory, "");
            }

            return directoryes;
        }

        private async Task<DownloadCommandDto> ParserCmdArguments(ITelegramBotClient botClient, Update update, UserCommand command)
        {
            //FluentCommandLineParser bag
            for (int i = 0; i < command.Args.Count; i++)
            {
                if (command.Args[i].StartsWith("/"))
                {
                    command.Args[i] = "~" + command.Args[i];
                }
            }

            var cmdParser = new FluentCommandLineParser<DownloadCommandDto>();
            cmdParser.Setup(arg => arg.IsStandartDirectory).As('s', "standart-dir").SetDefault(false);
            cmdParser.Setup(arg => arg.Directory).As('d', "dir").SetDefault(null);
            cmdParser.Setup(arg => arg.IsMakeDirectory).As("mkdir").SetDefault(false);

            var cmdParserResult = cmdParser.Parse(command.Args.ToArray());
            if (cmdParserResult.HasErrors)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, cmdParserResult.ErrorText);
                throw new Exception(cmdParserResult.ErrorText);
            }

            var dto = cmdParser.Object;
            dto.DownloadMethod = command?.Args[0].ToLower();

            //FluentCommandLineParser bag
            if (dto.Directory is not null && dto.Directory.StartsWith("~"))
            {
                dto.Directory = dto.Directory.Replace("~", "");
            }

            return dto;
        }
    }
}
