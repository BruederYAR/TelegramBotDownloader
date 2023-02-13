using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotDownloader.Core.Entities;

namespace TelegramBotDownloader.Core.Handlers.Command
{
    public interface ICommandHandler
    {
        Task HandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, PostPool posts, UserCommand commands);
    }
}
