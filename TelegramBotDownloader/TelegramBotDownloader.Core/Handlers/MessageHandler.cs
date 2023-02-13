using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotDownloader.Core.Attributes;
using TelegramBotDownloader.Core.Entities;
using System.Linq;

namespace TelegramBotDownloader.Core.Handlers
{
    [TelegramUpdateHandler(UpdateType.Message)]
    public class MessageHandler : ITelegramBotUpdateHandler
    {
        private readonly ILogger<MessageHandler> _logger;
        private readonly CommandHandler _commandHandler;

        public MessageHandler(ILogger<MessageHandler> logger, CommandHandler commandHandler)
        {
            _logger = logger;
            _commandHandler = commandHandler;
        }

        public async Task HandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, PostPool postPull, UserCommand userCommand)
        {
            var message = update.Message;

            if (message is null)
            { 
                return;
            }

            if (message.Text is not null && message.Text.StartsWith("/"))
            {
                try
                {
                    await _commandHandler.HandleAsync(botClient, update, cancellationToken, postPull, userCommand);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                return;
            }

            if(postPull.Messages is null || postPull.Messages.Count == 0)
            {
                postPull.Messages = new List<Message>() { message };
                return;
            }

            var firstMessage = postPull.Messages.FirstOrDefault(o => o.Date == message.Date);
            if (firstMessage is not null)
            {
                postPull.Messages.Add(message);
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "The bot has already memorized the last post. To delete it, print /pool --delete");
                return;
            }
        }
    }
}
