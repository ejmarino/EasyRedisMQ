using EasyRedisMQ.Resolvers;
using System;
using System.Threading.Tasks;
using EasyRedisMQ.Models;
using EasyRedisMQ.Services;
using StackExchange.Redis.Extensions.Core;

namespace EasyRedisMQ.Factories
{
    public class SubscriberFactory : ISubscriberFactory
    {
        private IKeyResolver keyResolver;
        private ICacheClient cacheClient;
        private IExchangeSubscriberService subscriberService;

        public SubscriberFactory(IKeyResolver keyResolver, 
            ICacheClient cacheClient, 
            IExchangeSubscriberService subscriberService)
        {
            this.keyResolver = keyResolver;
            this.cacheClient = cacheClient;
            this.subscriberService = subscriberService;
        }

        public async Task<Subscriber<T>> CreateSubscriberAsync<T>(string subscriberId, Func<T, Task> onMessageAsync) where T : class
        {
            var exchangeName = keyResolver.GetExchangeKey(typeof(T));
            var queueName = keyResolver.GetQueueKey(exchangeName, subscriberId);
            var subscriber = new Subscriber<T>(cacheClient, subscriberService)
            {
                SubscriberInfo = new SubscriberInfo
                {
                    SubscriberId = subscriberId,
                    ExchangeKey = exchangeName,
                    QueueKey = queueName
                },
                OnMessageAsync = onMessageAsync
            };

            await subscriber.InitializeAsync();

            return subscriber;
        }
    }
}
