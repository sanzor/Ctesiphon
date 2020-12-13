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
using Serilog;

namespace PubSubSharp.Server {
    public class SocketWare {
        private RequestDelegate next;
        private RedisStore store;
        private ILogger logger = Log.ForContext<SocketWare>();
        public SocketWare(RequestDelegate _next,RedisStore store) {
            this.next = _next;
            this.store = store;
        }
        public async Task Invoke(HttpContext context) {
            if (!context.WebSockets.IsWebSocketRequest) {
                return;
            }
            try {
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                var client = new ChatClient(socket, this.store);
                await client.RunAsync();
            } catch (Exception ex) {
                logger.Error($"Error:{ex.Message}");
                throw;
            }
           
            
        }
        

    }
}
