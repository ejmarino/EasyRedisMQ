using EasyRedisMQ.Factories;
using EasyRedisMQ.Models;
using EasyRedisMQ.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace EasyRedisMQ
{
    public class MessageBroker : IMessageBroker
    {
        private const string DefaultSubscriberId = "default";
        private IExchangeSubscriberService exchangeSubscriberService;
        private INotificationService notificationService;
        private ISubscriberFactory subscriberFactory;
        private MemoryCache memoryCache;

        public MessageBroker(IExchangeSubscriberService exchangeSubscriberService, INotificationService notificationService, ISubscriberFactory subscriberFactory)
        {
            this.exchangeSubscriberService = exchangeSubscriberService;
            this.notificationService = notificationService;
            this.subscriberFactory = subscriberFactory;
            memoryCache = MemoryCache.Default;
        }

        public async Task PublishAsync(object message) =>
            await PublishAsync(message, message.GetType());

        public async Task PublishAsync(object message, Type type)
        {
            var subscriberInfos = await GetSubscriberInfosAsync(type);

            var tasks = subscriberInfos.Select(subscriberInfo =>
            {
                return exchangeSubscriberService.PushMessageToSubscriberAsync(subscriberInfo, message);
            }).ToList();

            await Task.WhenAll(tasks);
            await notificationService.NotifyOfNewMessagesAsync(message);
        }

        public async Task<Subscriber<T>> SubscribeAsync<T>(Func<T, Task> onMessageAsync) where T : class =>
           await SubscribeAsync<T>(DefaultSubscriberId, onMessageAsync);

        public async Task<Subscriber<T>> SubscribeAsync<T>(string subscriberId, Func<T, Task> onMessageAsync) where T : class
        {
            var subScriber = await subscriberFactory.CreateSubscriberAsync(subscriberId, onMessageAsync);
            await AddSubscriberAsync(subScriber);
            return subScriber;
        }

        private async Task AddSubscriberAsync<T>(Subscriber<T> subscriber) where T : class =>
            await exchangeSubscriberService.AddSubscriberAsync(subscriber);

        private async Task<List<SubscriberInfo>> GetSubscriberInfosAsync(Type type)
        {
            var typeName = type.FullName;
            if (memoryCache[typeName] is List<SubscriberInfo> cachedSubscriberInfos)
                return cachedSubscriberInfos;
            var subscriberInfos = await exchangeSubscriberService.GetSubscriberInfosAsync(type);
            memoryCache.Add(typeName, subscriberInfos, DateTimeOffset.Now.AddSeconds(15));
            return subscriberInfos;
        }
    }
}
