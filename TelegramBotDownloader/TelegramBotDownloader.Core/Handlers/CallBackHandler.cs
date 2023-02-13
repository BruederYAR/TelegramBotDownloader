using System.Text.Json;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotDownloader.Core.Attributes;
using TelegramBotDownloader.Core.Entities;
using TelegramBotDownloader.Core.Handlers.Command;

namespace TelegramBotDownloader.Core.Handlers
{
    [TelegramUpdateHandler(UpdateType.CallbackQuery)]
    public class CallBackHandler : ITelegramBotUpdateHandler
    {
        private readonly ILogger<CallBackHandler> _logger;
        private readonly CommandParser _commandParser;

        public CallBackHandler(ILogger<CallBackHandler> logger, CommandParser commandParser)
        {
            _logger = logger;
            _commandParser = commandParser;
        }

        public async Task HandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, PostPool postPull, UserCommand command)
        {
            var callBackQuery = update.CallbackQuery;
            if (callBackQuery is null || postPull is null)
            {
                return;
            }

            if (command is null)
            {
                return;
            }
            
            _commandParser.TryParseCallBack(command.Name, out var commandCallBack);
            await commandCallBack.HandleCallBackAsync(botClient, update, cancellationToken, postPull, command);
        }
    }
}
