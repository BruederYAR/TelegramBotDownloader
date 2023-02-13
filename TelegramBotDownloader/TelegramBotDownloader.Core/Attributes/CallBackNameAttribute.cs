namespace TelegramBotDownloader.Core.Attributes
{
    public class CallBackNameAttribute : Attribute
    {
        public string Name { get; }
        public CallBackNameAttribute(string CallBackName)
        {
            Name = CallBackName;
        }
    }
}
