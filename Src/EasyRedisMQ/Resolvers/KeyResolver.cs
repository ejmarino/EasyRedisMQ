using System;

namespace EasyRedisMQ.Resolvers
{
    public class KeyResolver : IKeyResolver
    {
        public static string BaseExchangeKey = "MQ_Exchange";
        public static string Separator = ":";
        public static Func<Type, string> TypeNameFunc = type => type.Name;

        public string GetExchangeKey(Type messageType) => $"{BaseExchangeKey}{Separator}{TypeNameFunc(messageType)}";

        public string GetQueueKey(string exchangeName, string subscriberId) => $"{exchangeName}{Separator}Queue{Separator}{subscriberId}";
        
        public string GetSubscriberKey(Type messageType) => $"{GetExchangeKey(messageType)}{Separator}Subscribers";
    }
}
