using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using TelegramBotDownloader.Core.Entities;

namespace TelegramBotDownloader.UI
{
    internal class Worker : BackgroundService
    {
        private readonly IUpdateHandler _updateHandler;
        private readonly Options _options;
        private readonly TelegramBotClient _botClient;

        public Worker(IUpdateHandler updateHandler, Options options)
        {
            _updateHandler = updateHandler;
            _options = options;
            _botClient = new TelegramBotClient(_options.TelegramToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _botClient.StartReceiving(_updateHandler);
            return Task.CompletedTask;
        }
    }
}
