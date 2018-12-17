﻿using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Newtonsoft.Json;
using Redis.WebJobs.Extensions.Bindings;
using Redis.WebJobs.Extensions.Services;
using Redis.WebJobs.Extensions.Trigger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Redis.WebJobs.Extensions
{
    public class RedisConfiguration : IExtensionConfigProvider
    {
        private readonly INameResolver _resolver;
        internal const string AzureWebJobsRedisConnectionStringSetting = "AzureWebJobsRedisConnectionString";
        

        public RedisConfiguration(INameResolver resolver)
        {
            _resolver = resolver;
            LastValueKeyNamePrefix = "Previous_";
            CheckCacheFrequency = TimeSpan.FromSeconds(30);

            RedisServiceFactory = new DefaultRedisServiceFactory();
        }

        internal IRedisServiceFactory RedisServiceFactory { get; set; }

        public string ConnectionString { get; set; }
        public TimeSpan? CheckCacheFrequency { get; set; }
        public string LastValueKeyNamePrefix { get; set; }

        private void SetConnectionString(string settingName)
        {
            //ConnectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(settingName);
        }

        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ConnectionString = _resolver.Resolve($"ConnectionStrings:{AzureWebJobsRedisConnectionStringSetting}");
            ConnectionString = ConnectionString ?? _resolver.Resolve(AzureWebJobsRedisConnectionStringSetting);

            var bindingRule = context.AddBindingRule<RedisAttribute>();
            bindingRule.AddValidator(ValidateConnection);
            bindingRule.BindToCollector<RedisMessageOpenType>(typeof(RedisOpenTypeConverter<>), this);
            bindingRule.BindToValueProvider<RedisMessageOpenType>((attr, t) => BindForItemAsync(attr, t));

            var triggerRule = context.AddBindingRule<RedisTriggerAttribute>();
            triggerRule.BindToTrigger<string>(new RedisTriggerAttributeBindingProvider(this));
            triggerRule.AddOpenConverter<string, RedisMessageOpenType>(typeof(RedisMessageOpenTypeBindingConverter<>));
        }

        internal Task<IValueBinder> BindForItemAsync(IRedisAttribute attribute, Type type)
        {
            var context = CreateContext(attribute);

            Type genericType = typeof(RedisCacheValueBinder<>).MakeGenericType(type);
            IValueBinder binder = (IValueBinder)Activator.CreateInstance(genericType, context);

            return Task.FromResult(binder);
        }

        internal void ValidateConnection(RedisAttribute attribute, Type paramType)
        {
            if (string.IsNullOrEmpty(ConnectionString) &&
                string.IsNullOrEmpty(attribute.ConnectionStringSetting))
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture,
                    "The Redis connection string must be set either via a '{0}' app setting, via the RedisAttribute.ConnectionStringSetting property or via RedisConfiguration.ConnectionString.",
                    AzureWebJobsRedisConnectionStringSetting));
            }
        }
        internal string ResolveConnectionString(string attributeConnectionString)
        {
            // First, try the Attribute's string.
            if (!string.IsNullOrEmpty(attributeConnectionString))
            {
                return attributeConnectionString;
            }

            // Second, try the config's ConnectionString
            return ConnectionString;
        }
        internal IRedisService CreateService(IRedisAttribute attribute)
        {
            string resolvedConnectionString = ResolveConnectionString(attribute.ConnectionStringSetting);
            return RedisServiceFactory.CreateService(resolvedConnectionString);
        }

        internal RedisContext CreateContext(IRedisAttribute attribute)
        {
            IRedisService service = CreateService(attribute);

            return new RedisContext
            {
                ResolvedAttribute = attribute,
                Service = service
            };
        }

        private class RedisMessageOpenType : OpenType.Poco
        {
            public override bool IsMatch(Type type, OpenTypeMatchContext context)
            {
                if (type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return false;
                }

                if (type.FullName == "System.Object")
                {
                    return true;
                }

                return base.IsMatch(type, context);
            }
        }

        private class RedisMessageOpenTypeBindingConverter<TOutput>
             : IConverter<string, TOutput>
        {
            public TOutput Convert(string input)
            {
                if (input != null)
                {
                    return JsonConvert.DeserializeObject<TOutput>(input);
                }
                return default(TOutput);
            }
        }
    }
}
