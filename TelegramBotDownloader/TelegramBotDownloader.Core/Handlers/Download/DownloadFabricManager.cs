using System.Reflection;
using TelegramBotDownloader.Core.Attributes;
using TelegramBotDownloader.Core.Entities;
using TelegramBotDownloader.Core.Handlers.Download.Methods;

namespace TelegramBotDownloader.Core.Handlers.Download
{
    public class DownloadFabricManager
    {
        public DownloadFabricManager()
        {
            
        }

        public DownloadFabric GetDownloadMethod(string methodName, string downloadDir)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException("CallBack can not be null");
            }

            if (methodName == GetCallbackNameFromAttribute(typeof(DownloadMedia)))
            {
                return new DownloadMedia(downloadDir);
            }
            if (methodName == GetCallbackNameFromAttribute(typeof(DownloadText)))
            {
                return new DownloadText(downloadDir);
            }

            return new DownloadPost(downloadDir);
        }

        public static string GetCallbackNameFromAttribute(Type type)
        {
            var arrtibute = type.GetCustomAttribute<DownloadMethodAttribute>();

            if (arrtibute is null)
            {
                throw new ArgumentException($"attribute {nameof(DownloadMethodAttribute)} not found");
            }

            return arrtibute.Name.ToLower();
        }
    }
}
