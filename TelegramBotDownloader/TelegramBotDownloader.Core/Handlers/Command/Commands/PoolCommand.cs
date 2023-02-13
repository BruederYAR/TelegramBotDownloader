using Fclp;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotDownloader.Core.Attributes;
using TelegramBotDownloader.Core.Entities;
using TelegramBotDownloader.Core.Entities.Dto;

namespace TelegramBotDownloader.Core.Handlers.Command.Commands
{
    [Command("/pool")]
    internal class PoolCommand : ICommandHandler
    {
        private readonly ILogger<PoolCommand> _logger;

        public PoolCommand(ILogger<PoolCommand> logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, PostPool posts, UserCommand command)
        {
            if (command.Args.Count == 0)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Post count: {(posts is null ? 0 : posts.Messages.Count)}");
                return;
            }

            var dto = await ParseCmdArguments(botClient, update, command);

            if (dto.IsDelete)
            {
                await DeletePosts(botClient, update, posts);
            }
        }

        private async Task<PoolCommandDto> ParseCmdArguments(ITelegramBotClient botClient, Update update, UserCommand command)
        {
            var cmdParser = new FluentCommandLineParser<PoolCommandDto>();
            cmdParser.Setup(arg => arg.IsDelete).As('d', "delete").SetDefault(false);


            var cmdParserResult = cmdParser.Parse(command.Args.ToArray());
            if (cmdParserResult.HasErrors)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, cmdParserResult.ErrorText);
                throw new Exception(cmdParserResult.ErrorText);
            }

            return cmdParser.Object;
        }

        private async Task DeletePosts(ITelegramBotClient botClient, Update update, PostPool post)
        {
            var chatid = update.Message is null ? update.CallbackQuery.Message.Chat.Id : update.Message.Chat.Id;

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

            await botClient.SendTextMessageAsync(chatid, "Post was deleted");
        }
    }
}
