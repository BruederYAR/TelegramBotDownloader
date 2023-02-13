namespace TelegramBotDownloader.Core.Attributes
{
    public class CommandAttribute : Attribute
    {
        public string CommandName { get; }
        public CommandAttribute(string commandName)
        {
            CommandName = commandName;
        }
    }
}
