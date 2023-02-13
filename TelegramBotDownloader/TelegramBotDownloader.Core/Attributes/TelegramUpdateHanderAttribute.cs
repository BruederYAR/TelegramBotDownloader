using Telegram.Bot.Types.Enums;

namespace TelegramBotDownloader.Core.Attributes
{
    public class TelegramUpdateHandlerAttribute : Attribute
    {
        public UpdateType UpdateType { get; }

        public TelegramUpdateHandlerAttribute(UpdateType updateType)
        {
            UpdateType = updateType;
        }
    }
}
