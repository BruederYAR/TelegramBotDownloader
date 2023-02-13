namespace TelegramBotDownloader.Core.Attributes
{
    public class DownloadMethodAttribute : Attribute
    {
        public string Name { get; set; }
        public DownloadMethodAttribute(string name)
        {
            Name = name;
        }
    }
}
