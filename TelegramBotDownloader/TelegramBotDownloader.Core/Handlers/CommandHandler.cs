using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotDownloader.Core.Attributes;
using TelegramBotDownloader.Core.Entities;
using TelegramBotDownloader.Core.Handlers.Command;

namespace TelegramBotDownloader.Core.Handlers
{
    [TelegramUpdateHandler(UpdateType.Unknown)]
    public class CommandHandler : ITelegramBotUpdateHandler
    {
        private readonly ILogger<CommandHandler> _logger;
        private readonly CommandParser _commandParser;
        public CommandHandler(ILogger<CommandHandler> logger, CommandParser commandParser)
        {
            _logger = logger;
            _commandParser = commandParser;
        }

        public async Task HandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, PostPool postPull, UserCommand userCommand)
        {
            var message = update.Message;

            var splitText = message.Text.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var commandName = splitText[0];
            var commandArguments = splitText.Skip(1).ToArray();

            for (int i = 0; i < commandArguments.Length; i++)
            {
                commandArguments[i] = commandArguments[i].Replace("—", "--");
            }

            userCommand.Name = commandName;
            userCommand.Args = commandArguments.ToList();


            if (_commandParser.TryParse(userCommand.Name, out var command))
            {
                await command.HandleAsync(botClient, update, cancellationToken, postPull, userCommand);
            }

        }
    }
}
