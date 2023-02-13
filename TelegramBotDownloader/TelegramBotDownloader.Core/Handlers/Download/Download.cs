using Telegram.Bot;
using TelegramBotDownloader.Core.Entities;

namespace TelegramBotDownloader.Core.Handlers.Download
{
    public class DownloadFabric
    {
        public string DownloadDirectory { get; protected set; }

        public DownloadFabric(string path)
        {
            DownloadDirectory = path;
        }

        public virtual Task DownloadAsync(ITelegramBotClient botClient, CancellationToken cancellationToken, PostPool post)
        {
            throw new NotImplementedException();
        }

        protected static string GetFileName(string startWith)
        {
            var now = DateTime.UtcNow;
            return $"{System.IO.Path.DirectorySeparatorChar}{startWith}{now.Day}.{now.Month}.{now.Year}_{now.Hour}.{now.Minute}.{now.Second}.{now.Millisecond}";
        }

        protected async Task<string> DownloadFile(ITelegramBotClient botClient, CancellationToken cancellationToken, string fileId, string dir)
        {
            var fileInfo = await botClient.GetFileAsync(fileId);
            var filePath = fileInfo.FilePath;
            var fileExtention = filePath?.Split('.').Last();

            var fullPath = $"{dir}{System.IO.Path.DirectorySeparatorChar}";
            CreateDirectory(fullPath);

            string destinationFilePath = $"{fullPath}{fileInfo.FileUniqueId}.{fileExtention}";

            await using Stream fileStream = System.IO.File.OpenWrite(destinationFilePath);
            await botClient.DownloadFileAsync(
                filePath: filePath,
                destination: fileStream,
                cancellationToken: cancellationToken);

            return $"{fileInfo.FileUniqueId}.{fileExtention}";
        }
        protected async Task<string> DownloadFile(ITelegramBotClient botClient, CancellationToken cancellationToken, string fileId)
        {
            return await DownloadFile(botClient, cancellationToken, fileId, DownloadDirectory);
        }

        protected void CreateDirectory(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
