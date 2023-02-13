using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace TelegramBotDownloader.UI.Extensions
{
    public static class SerilogExtension
    {
        public static IHostBuilder ConfigureApiSerilogLogging(this IHostBuilder builder) => builder
                .ConfigureLogging((loggingBuilder) =>
                {
                    loggingBuilder.ClearProviders();
                })
                .UseSerilog((context, services, configuration) =>
                {
                    configuration.ReadFrom.Configuration(context.Configuration);
                });

    }

}
