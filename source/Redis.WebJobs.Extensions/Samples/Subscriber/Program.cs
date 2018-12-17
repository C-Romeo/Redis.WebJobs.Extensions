using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Redis.WebJobs.Extensions;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Subscriber
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .UseEnvironment("Development")
                .ConfigureWebJobs(b =>
                {
                    b.UseRedis();
                    //b.UseHostId("ecad61-62cf-47f4-93b4-6efcded6")
                    //    .AddWebJobsLogging() // Enables WebJobs v1 classic logging 
                    //    .AddAzureStorageCoreServices();
                })
                .ConfigureAppConfiguration(b =>
                {
                    // Adding command line as a configuration source
                    //b.AddCommandLine(args);
                })
                .ConfigureLogging((context, b) =>
                {
                    b.SetMinimumLevel(LogLevel.Debug);
                    b.AddConsole();

                    // If this key exists in any config, use it to enable App Insights
                    string appInsightsKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                    if (!string.IsNullOrEmpty(appInsightsKey))
                    {
                        //b.AddApplicationInsights(o => o.InstrumentationKey = appInsightsKey);
                    }
                })
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}
