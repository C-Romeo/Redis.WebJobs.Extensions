
using Microsoft.Azure.WebJobs;
using System;
using Microsoft.Extensions.Configuration;

namespace Redis.WebJobs.Extensions
{
    public static class RedisJobHostConfigurationExtensions
    {
        public static void UseRedis(this IWebJobsBuilder b)
        {
            if (b == null)
            {
                throw new ArgumentNullException("config");
            }


            b.AddExtension<RedisConfiguration>().ConfigureOptions<RedisConfiguration>((config, options) =>
            {
                config.Bind(options);
                options.ConnectionString =
                    config.GetConnectionString(RedisConfiguration.AzureWebJobsRedisConnectionStringSetting);
                //options.ConnectionString
            });
        }

        
    }
}
