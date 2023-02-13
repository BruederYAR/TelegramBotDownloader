using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TelegramBotDownloader.Core.Entities;
using TelegramBotDownloader.Core.Handlers;
using TelegramBotDownloader.Core.Handlers.Download;

namespace TelegramBotDownloader.Core
{
    public class TelegramBotMessageHandler : IUpdateHandler
    {
        private readonly ILogger<TelegramBotMessageHandler> _logger;
        private readonly UpdateHandlersManager _updateHandlerManager;
        private readonly Options _options;
        private PostPool _postPull = new();
        private UserCommand _commandQueue = new();

        public TelegramBotMessageHandler(ILogger<TelegramBotMessageHandler> logger, UpdateHandlersManager updateHandlerManager, Options options)
        {
            _logger = logger;
            _updateHandlerManager = updateHandlerManager;
            _options = options;
            _postPull = new PostPool();
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception.Message);
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                var handler = _updateHandlerManager.GetHandler(update.Type);
                handler?.HandleAsync(botClient, update, cancellationToken, _postPull, _commandQueue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
