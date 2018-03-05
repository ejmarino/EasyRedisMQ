using EasyRedisMQ.Resolvers;
using StackExchange.Redis.Extensions.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRedisMQ.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ICacheClient cacheClient;
        private readonly IKeyResolver nameResolver;

        public NotificationService(ICacheClient cacheClient, IKeyResolver nameResolver)
        {
            this.cacheClient = cacheClient;
            this.nameResolver = nameResolver;
        }

        public async Task NotifyOfNewMessagesAsync(object message)
        {
            var exchangeName = nameResolver.GetExchangeKey(message.GetType());
            await cacheClient.PublishAsync(exchangeName, exchangeName);
        }
    }
}
