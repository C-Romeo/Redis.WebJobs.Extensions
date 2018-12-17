using Microsoft.Azure.WebJobs;
using Redis.WebJobs.Extensions;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Publisher
{
    class Program
    {
        static void Main(string[] args)
        {

            var builder = new HostBuilder()
                .UseEnvironment("Development")
                .ConfigureWebJobs(b =>
                {
                    b.UseHostId("ecad61-62cf-47f4-93b4-6efcded6")
                        .AddWebJobsLogging() // Enables WebJobs v1 classic logging 
                        .AddAzureStorageCoreServices();

                    b.UseRedis();
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
                host.RunAsync();
            }

            // Give subscriber chance to startup
            Task.Delay(5000).Wait();

            // host.Call(typeof(Functions).GetMethod("SendStringMessage"));
            //Task.Delay(5000).Wait();

            //host.Call(typeof(Functions).GetMethod("SendMultipleStringPubSubMessages"));
            //Task.Delay(5000).Wait();

            //host.Call(typeof(Functions).GetMethod("SendPocoMessage"));
            //Task.Delay(5000).Wait();

            //host.Call(typeof(Functions).GetMethod("SetStringToCache"));
            //Task.Delay(5000).Wait();

            //host.Call(typeof(Functions).GetMethod("SetStringToCacheUsingResolver"));
            //Task.Delay(5000).Wait();

            //host.Call(typeof(Functions).GetMethod("SetPocoToCache"));
            //Task.Delay(5000).Wait();

            //Console.CancelKeyPress += (sender, e) =>
            //{
            //    host.Stop();
            //};
            
            ////host.RunAndBlock();

        }
    }
}
