using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotDownloader.Core.Entities;

namespace TelegramBotDownloader.Core.Handlers
{
    public interface ITelegramBotUpdateHandler
    {
        Task HandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, PostPool postPull, UserCommand commands);
    }
}
