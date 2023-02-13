using System.Text;
using Telegram.Bot;
using TelegramBotDownloader.Core.Attributes;
using TelegramBotDownloader.Core.Entities;

namespace TelegramBotDownloader.Core.Handlers.Download.Methods
{
    [DownloadMethod("Post")]
    internal class DownloadPost : DownloadFabric
    {
        public DownloadPost(string path) : base(path) { }

        public override async Task DownloadAsync(ITelegramBotClient botClient, CancellationToken cancellationToken, PostPool post)
        {
            string postDirectory = this.DownloadDirectory + GetFileName("Post_");
            this.CreateDirectory(postDirectory);

            StringBuilder startBuilder = new StringBuilder();
            StringBuilder middleBuilder = new StringBuilder();
            StringBuilder endBuilder = new StringBuilder();
            foreach (var message in post.Messages)
            {
                if (message is null)
                {
                    continue;
                }

                if (message.Photo is not null)
                {
                    var filename = await DownloadFile(botClient, cancellationToken, message.Photo.Last().FileId, postDirectory);
                    startBuilder.AppendLine($"![photo{filename}](.{Path.DirectorySeparatorChar}{filename})");
                }
                if(message.Video is not null)
                {
                    var filename = await DownloadFile(botClient, cancellationToken, message.Video.FileId, postDirectory);
                    startBuilder.AppendLine($"<video src=\".{Path.DirectorySeparatorChar}{filename}\"></video>");
                }
                if (message.Caption is not null)
                {
                    middleBuilder.AppendLine(message.Caption);
                }
                if (message.Text is not null)
                {
                    middleBuilder.AppendLine(message.Text);
                }
                if (message.Document is not null)
                {
                    var filename = await DownloadFile(botClient, cancellationToken, message.Document.FileId, postDirectory);
                    endBuilder.AppendLine($"[{message.Document.FileName}](.{Path.DirectorySeparatorChar}{filename})");
                }
            }

            using (var sw = new StreamWriter(postDirectory + Path.DirectorySeparatorChar + "Post.md", false))
            {
                await sw.WriteLineAsync(startBuilder.ToString());
                await sw.WriteLineAsync(middleBuilder.ToString());
                await sw.WriteLineAsync(endBuilder.ToString());
            }
        }
    }
}
