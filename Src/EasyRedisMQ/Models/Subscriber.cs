using EasyRedisMQ.Extensions;
using EasyRedisMQ.Services;
using StackExchange.Redis.Extensions.Core;
using System;
using System.Threading.Tasks;

namespace EasyRedisMQ.Models
{
    public abstract class SubscriberBase
    {
        protected ICacheClient cacheClient;

        public SubscriberInfo SubscriberInfo { get; set; }

        protected SubscriberBase(ICacheClient cacheClient)
        {
            this.cacheClient = cacheClient;
        }

        public virtual async Task InitializeAsync()
        {
            if (SubscriberInfo == null) throw new NullReferenceException("SubscriberInfo is required.");
            if (string.IsNullOrWhiteSpace(SubscriberInfo.SubscriberId)) throw new NullReferenceException("SubscriberId is required");
            if (string.IsNullOrWhiteSpace(SubscriberInfo.ExchangeKey)) throw new NullReferenceException("ExchangeName is required");
            if (string.IsNullOrWhiteSpace(SubscriberInfo.QueueKey)) throw new NullReferenceException("QueueName is required");

            await cacheClient.SubscribeAsync<string>(SubscriberInfo.ExchangeKey, DoWorkAsync);

            DoWorkAsync("").FireAndForget();
        }

        protected abstract Task DoWorkAsync(string arg);

        public async Task UnsubscribeAsync() =>
            await cacheClient.UnsubscribeAsync<string>(SubscriberInfo.ExchangeKey, DoWorkAsync);

    }

    public class Subscriber<T> : SubscriberBase where T : class
    {
        private IExchangeSubscriberService exchangeSubscriberService;

        public Subscriber(ICacheClient cacheClient, IExchangeSubscriberService exchangeSubscriberService)
            : base(cacheClient)
        {
            this.exchangeSubscriberService = exchangeSubscriberService;
        }

        public Func<T, Task> OnMessageAsync { get; set; }

        private async Task<T> GetNextMessageAsync() =>
            await cacheClient.ListGetFromRightAsync<T>(SubscriberInfo.QueueKey);

        private async Task PushAsync(T message) =>
            await exchangeSubscriberService.PushMessageToSubscriberAsync(SubscriberInfo, message);

        public override async Task InitializeAsync()
        {
            if (OnMessageAsync == null) throw new NullReferenceException("OnMessageAsync is required");
            await base.InitializeAsync();
        }

        protected override async Task DoWorkAsync(string arg)
        {
            int numberOfMessagesProcessed = 0;
            while (true)
            {
                var message = await GetNextMessageAsync();
                if (message == null) break;

                numberOfMessagesProcessed++;
                try
                {
                    await OnMessageAsync(message);
                }
                catch (Exception)
                {
                    await PushAsync(message);
                    throw;
                }
            }
        }
    }
}
