using System.Collections.Generic;
using System.Threading.Tasks;
using EasyRedisMQ.Models;
using StackExchange.Redis.Extensions.Core;
using EasyRedisMQ.Resolvers;
using System.Linq;
using System;

namespace EasyRedisMQ.Services
{
    public class ExchangeSubscriberService : IExchangeSubscriberService
    {
        private readonly ICacheClient cacheClient;
        private IKeyResolver keyResolver;

        public ExchangeSubscriberService(ICacheClient cacheClient, IKeyResolver keyResolver)
        {
            this.cacheClient = cacheClient;
            this.keyResolver = keyResolver;
        }

        public async Task AddSubscriberAsync<T>(Subscriber<T> subScriber) where T : class
        {
            var subscriberKey = keyResolver.GetSubscriberKey(typeof(T));
            var wasSuccessful = await cacheClient.SetAddAsync<SubscriberInfo>(subscriberKey, subScriber.SubscriberInfo);
        }

        public async Task<List<SubscriberInfo>> GetSubscriberInfosAsync(Type type)
        {
            var subscriberKey = keyResolver.GetSubscriberKey(type);
            var subscribers = await cacheClient.SetMembersAsync<SubscriberInfo>(subscriberKey);
            return subscribers.ToList();
        }

        public async Task<List<SubscriberInfo>> GetSubscriberInfosAsync<T>() where T : class
        {
            var subscriberKey = keyResolver.GetSubscriberKey(typeof(T));
            var subscribers = await cacheClient.SetMembersAsync<SubscriberInfo>(subscriberKey);
            return subscribers.ToList();
        }

        public async Task PushMessageToSubscriberAsync<T>(SubscriberInfo subscriberInfo, T message) where T : class
        {
            await cacheClient.ListAddToLeftAsync(subscriberInfo.QueueName, message);
        }
    }
}
