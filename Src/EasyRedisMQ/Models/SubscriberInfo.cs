using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRedisMQ.Models
{
    public class SubscriberInfo
    {
        public string SubscriberId { get; set; }
        public string ExchangeKey { get; set; }
        public string QueueKey { get; set; }
        public string Topic { get; set; }
    }
}
