using Telegram.Bot;
using TelegramBotDownloader.Core.Attributes;
using TelegramBotDownloader.Core.Entities;

namespace TelegramBotDownloader.Core.Handlers.Download.Methods
{
    [DownloadMethod("Media")]
    internal class DownloadMedia : DownloadFabric
    {
        public DownloadMedia(string path) : base(path) { }

        public override async Task DownloadAsync(ITelegramBotClient botClient, CancellationToken cancellationToken, PostPool post)
        {
            if (post is null)
            {
                throw new ArgumentNullException(nameof(post));
            }

            foreach (var message in post.Messages)
            {
                var fileid = message.Photo?.Last().FileId + message.Video?.FileId + message.Document?.FileId;
                if (!string.IsNullOrEmpty(fileid))
                {
                    await DownloadFile(botClient, cancellationToken, fileid);
                }
            }
        }
    }
}
