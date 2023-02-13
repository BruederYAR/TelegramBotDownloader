using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotDownloader.Core
{
    internal class Menu
    {
        private readonly List<InlineKeyboardButton> _buttons;
        private readonly ITelegramBotClient _botClient;
        private readonly long _chatId;
        private readonly string _title;

        public Menu(ITelegramBotClient botClient, long chatId, string title = "Menu")
        {
            _buttons = new List<InlineKeyboardButton>();
            _botClient = botClient;
            _chatId = chatId;   
            _title = title;
        }

        public void AddButton(string Title, string CallbackQuery)
        {
            if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(CallbackQuery))
            {
                throw new ArgumentNullException($"{nameof(Title)} or {nameof(CallbackQuery)}");
            }

            _buttons.Add(InlineKeyboardButton.WithCallbackData(Title, CallbackQuery));
            int a = 0;
        }

        public async Task<int> Show()
        {
            var keyboard = GetKeyboard(2);
            var message = await _botClient.SendTextMessageAsync(_chatId, _title, replyMarkup: keyboard);
            return message.MessageId;
        }

        private InlineKeyboardMarkup GetKeyboard(int step)
        {
            var rows = new List<List<InlineKeyboardButton>>();
            var cols = new List<InlineKeyboardButton>();
            int stepCount = 0;

            for (int i = 0; i < _buttons.Count; i++)
            {
                cols.Add(_buttons[i]);
                stepCount++;

                if(step == stepCount || _buttons[i].Text.Length > 22)
                {
                    stepCount = 0;
                    rows.Add(cols);
                    cols = new List<InlineKeyboardButton>();
                }
            }

            if(_buttons.Count == 1)
            {
                return new InlineKeyboardMarkup(_buttons[0]);
            }

            return new InlineKeyboardMarkup(rows);
        }
    }
}
