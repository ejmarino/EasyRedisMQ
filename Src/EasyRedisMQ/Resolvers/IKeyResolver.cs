using System;

namespace EasyRedisMQ.Resolvers
{
    public interface IKeyResolver
    {
        string GetExchangeKey(Type messagetype);

        string GetQueueKey(string exchangeName, string subscriberId);

        string GetSubscriberKey(Type messageType);

    }
}
