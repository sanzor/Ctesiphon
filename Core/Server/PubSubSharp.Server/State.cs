using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace PubSub.Server {
    internal class State {
        public string ClientId { get; set; }
        public WebSocket socket;
        public Task outboundTask;
        public ISubscriber subscriber;
        public IDatabase redisDB;

    }
}
