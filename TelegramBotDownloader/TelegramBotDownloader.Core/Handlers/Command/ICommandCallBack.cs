using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotDownloader.Core.Entities;

namespace TelegramBotDownloader.Core.Handlers.Command
{
    public interface ICommandCallBack
    {
        Task HandleCallBackAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, PostPool post, UserCommand commands);
    }
}
