using Microsoft.Extensions.DependencyInjection;
using TelegramBotDownloader.Core.Handlers;
using TelegramBotDownloader.Core.Handlers.Command;

namespace TelegramBotDownloader.UI.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection RegisterTelegramDependencies(this IServiceCollection services)
        {
            var applicationTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(t => t.GetTypes());

            services.RegisterTelegramBotUpdateHandlers(applicationTypes)
                .RegisterTelegramBotCommandHandles(applicationTypes);

            return services;
        }

        private static IServiceCollection RegisterTelegramBotUpdateHandlers(this IServiceCollection services, IEnumerable<Type> applicationTypes)
        {
            var telegramBotUpdateHandlerAbstraction = typeof(ITelegramBotUpdateHandler);
            var telegramBotUpdateHandlerTypes = applicationTypes.Where(t => t.GetInterfaces().Any(i => i == telegramBotUpdateHandlerAbstraction));

            foreach (var telegramBotUpdateHandlerType in telegramBotUpdateHandlerTypes)
            {
                services.AddSingleton(telegramBotUpdateHandlerType);
            }

            return services;
        }

        private static IServiceCollection RegisterTelegramBotCommandHandles(this IServiceCollection services, IEnumerable<Type> applicationTypes)
        {
            var commandHandlerAbstraction = typeof(ICommandHandler);
            var commandHandlerTypes = applicationTypes.Where(t => t.GetInterfaces().Any(i => i == commandHandlerAbstraction));

            foreach (var commandHandlerType in commandHandlerTypes)
            {
                services.AddSingleton(commandHandlerType);
            }

            return services;
        }

    }
}
