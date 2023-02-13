using System.Text;
using Telegram.Bot;
using TelegramBotDownloader.Core.Attributes;
using TelegramBotDownloader.Core.Entities;

namespace TelegramBotDownloader.Core.Handlers.Download.Methods
{
    [DownloadMethod("Text")]
    internal class DownloadText : DownloadFabric
    {
        public DownloadText(string path) : base(path) { }

        public override async Task DownloadAsync(ITelegramBotClient botClient, CancellationToken cancellationToken, PostPool post)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var message in post.Messages)
            {
                if (message is null || string.IsNullOrEmpty(message.Text))
                {
                    continue;
                }
                var from = message.ForwardFrom is null ? message.ForwardSenderName : message.ForwardFrom.Username;

                stringBuilder.AppendLine($"From: @{from}");
                stringBuilder.AppendLine(message.Text);
                stringBuilder.AppendLine();
            }

            using (var sw = new StreamWriter(this.DownloadDirectory + GetFileName("Text_") + ".txt", false))
            {
                await sw.WriteLineAsync(stringBuilder.ToString());
            }
        }
    }
}
