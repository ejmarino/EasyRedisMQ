using EasyRedisMQ.Models;
using System;
using System.Threading.Tasks;

namespace EasyRedisMQ
{
    public interface IMessageBroker
    {
        /// <summary>
        /// Publish specified message. Exchange key is obtained from message class type.
        /// </summary>
        /// <param name="message">Message to publish</param>
        Task PublishAsync(object message);
        /// <summary>
        /// Publish specified message. Exchange key is obtained from messageType parameter.
        /// message object's type must be related to messageType.
        /// </summary>
        /// <param name="message">Message to publish</param>
        /// <param name="messageType">type of message used to get Exchange key</param>
        /// <returns></returns>
        Task PublishAsync(object message, Type messageType);
        Task<Subscriber<T>> SubscribeAsync<T>(Func<T, Task> onMessageAsync) where T : class;
        Task<Subscriber<T>> SubscribeAsync<T>(string subscriptionId, Func<T, Task> onMessageAsync) where T : class;
    }
}
