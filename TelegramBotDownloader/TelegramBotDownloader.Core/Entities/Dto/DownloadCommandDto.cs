namespace TelegramBotDownloader.Core.Entities.Dto
{
    internal class DownloadCommandDto
    {
        public string DownloadMethod { get; set; }
        public bool IsStandartDirectory { get; set; }
        public string? Directory { get; set; }
        public bool IsMakeDirectory { get; set; } 
    }
}
