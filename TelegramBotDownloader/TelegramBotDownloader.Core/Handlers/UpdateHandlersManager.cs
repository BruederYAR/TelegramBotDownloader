using System.Data;
using System.Reflection;
using Telegram.Bot.Types.Enums;
using TelegramBotDownloader.Core.Attributes;

namespace TelegramBotDownloader.Core.Handlers
{
    public class UpdateHandlersManager
    {
        private readonly Dictionary<UpdateType, ITelegramBotUpdateHandler> _handlersCache = new();
        private readonly IServiceProvider _serviceProvider;

        public UpdateHandlersManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var applicationTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .Where(t => t.GetInterfaces().Any(t => t == typeof(ITelegramBotUpdateHandler)));

            foreach (var type in applicationTypes)
            {
                var updateHandlerAttribute = type.GetCustomAttribute<TelegramUpdateHandlerAttribute>();
                if (updateHandlerAttribute == null)
                {
                    continue;
                }
                    
                if (_handlersCache.ContainsKey(updateHandlerAttribute.UpdateType))
                {
                    throw new InvalidConstraintException($"Repeating the command name: {updateHandlerAttribute.UpdateType}");
                }
                    
                _handlersCache.Add(updateHandlerAttribute.UpdateType, (ITelegramBotUpdateHandler)serviceProvider.GetService(type)!);
            }
        }

        public ITelegramBotUpdateHandler GetHandler(UpdateType updateType)
        {
            if (_handlersCache.TryGetValue(updateType, out var updateHandler))
            {
                return updateHandler;
            }

            return null;
        }
    }
}
