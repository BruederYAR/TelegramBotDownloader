using System.Data;
using System.Reflection;
using TelegramBotDownloader.Core.Attributes;

namespace TelegramBotDownloader.Core.Handlers.Command
{
    public class CommandParser
    {
        private readonly Dictionary<string, ICommandHandler> _commandsCache = new();
        private readonly Dictionary<string, ICommandCallBack> _commandsCallBackCache = new();

        public CommandParser(IServiceProvider serviceProvider)
        {
            var commandTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .Where(t => t.GetInterfaces().Any(t => t == typeof(ICommandHandler)));

            var commandCallBackTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .Where(t => t.GetInterfaces().Any(t => t == typeof(ICommandCallBack)));

            _commandsCache = Search<ICommandHandler>(serviceProvider, commandTypes);
            _commandsCallBackCache = Search<ICommandCallBack>(serviceProvider, commandCallBackTypes);
        }

        private Dictionary<string, T> Search<T>(IServiceProvider serviceProvider, IEnumerable<Type> types)
        {
            if (types is null)
            {
                throw new ArgumentNullException(nameof(types));
            }
            Dictionary<string, T> result = new Dictionary<string,T>();

            foreach (var type in types)
            {
                var textCommandAttribute = type.GetCustomAttribute<CommandAttribute>();
                if (textCommandAttribute == null)
                    continue;

                if (result.ContainsKey(textCommandAttribute.CommandName))
                    throw new InvalidConstraintException($"Repeating the command name: {textCommandAttribute.CommandName}");

                result.Add(textCommandAttribute.CommandName, (T)serviceProvider.GetService(type)!);
            }

            return result;
        }

        public bool TryParse(string commandText, out ICommandHandler commandHandler)
        {
            var commandName = commandText.Split(" ")[0];
            return _commandsCache.TryGetValue(commandName, out commandHandler!);
        }

        public bool TryParseCallBack(string commandText, out ICommandCallBack commandHandler)
        {
            return _commandsCallBackCache.TryGetValue(commandText, out commandHandler!);
        }
    }
}
