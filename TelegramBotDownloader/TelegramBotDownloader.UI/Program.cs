using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot.Polling;
using TelegramBotDownloader.Core;
using TelegramBotDownloader.Core.Entities;
using TelegramBotDownloader.Core.Handlers;
using TelegramBotDownloader.Core.Handlers.Command;
using TelegramBotDownloader.Core.Handlers.Download;
using TelegramBotDownloader.UI.Extensions;

namespace TelegramBotDownloader.UI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .ConfigureApiSerilogLogging()
                .Build()
                .Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, service) =>
                {
                    //Options
                    IConfiguration configuration = context.Configuration;
                    Options options = new Options() { DownloadFolder = configuration["Options:DownloadFolder"], TelegramToken = configuration["Options:TelegramToken"] };
                    if (string.IsNullOrEmpty(options.DownloadFolder))
                    {
                        options.DownloadFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}{Path.DirectorySeparatorChar}TelegramDownloadBot";
                    }
                    if (string.IsNullOrEmpty(options.TelegramToken))
                    {
                        throw new ArgumentNullException(nameof(options.TelegramToken));
                    }
                    service.AddSingleton(options);

                    //Managers
                    service.AddSingleton<CommandParser>();
                    service.AddSingleton<UpdateHandlersManager>();
                    service.AddSingleton<DownloadFabricManager>();

                    //Handlers
                    service.RegisterTelegramDependencies();

                    //Host
                    service.AddSingleton<IUpdateHandler, TelegramBotMessageHandler>();
                    service.AddHostedService<Worker>();

                    new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? ""}.json", true)
                        .Build();

                });
        }
        
    }
}