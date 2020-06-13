using Microsoft.AspNetCore.Http;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PubSubSharp.DataAccess;
using PubSubSharp.Extensions;
using PubSubSharp.Interfaces;
using PubSubSharp.Server.Core;

namespace PubSubSharp.Server {
    public class SocketWare {
        private RequestDelegate next;
        private RedisStore store;
        private IChannelSubscriptionService channelRegistry;
      
        public SocketWare(RequestDelegate _next,RedisStore store,IChannelSubscriptionService channelRegistry) {
            this.next = _next;
            this.store = store;
            this.channelRegistry = channelRegistry;
           
        }
        public async Task Invoke(HttpContext context) {
            if (!context.WebSockets.IsWebSocketRequest) {
                return;
            }
            var socket=await context.WebSockets.AcceptWebSocketAsync();
            await RunAsync(socket);
        }
        private async Task RunAsync(WebSocket socket) {
            try {
                var client = new ChatClient(socket, this.store,this.channelRegistry);
                await client.RunAsync();
            } catch (Exception ex) {

                throw;
            }
            
        }
        

    }
}
