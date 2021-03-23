using Microsoft.Extensions.Logging.Console;
using Zero.Extensions.Logging.Console;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ZeroLoggingServiceCollectionExtensions
    {
        public static IServiceCollection AddZeroJsonConsoleLogging(this IServiceCollection services)
        {
            var serviceType = typeof(ConsoleFormatter);

            // Remove existing
            int count = services.Count;
            for (int i = 0; i < count; i++)
            {
                if (services[i].ServiceType == serviceType && services[i].ImplementationType?.Name == "JsonConsoleFormatter")
                {
                    services.RemoveAt(i);
                    break;
                }
            }

            services.AddSingleton<ConsoleFormatter, ZeroJsonConsoleFormatter>();

            return services;
        }
    }
}
