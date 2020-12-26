using Microsoft.AspNetCore.Http;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using StackExchange.Redis;

namespace PubSubSharp.Server {
    public class SocketWare {
        private RequestDelegate next;
        private ConnectionMultiplexer mux;
        public SocketWare(RequestDelegate _next, ConnectionMultiplexer mux) {
            this.next = _next;
            this.mux = mux;
        }
        public async Task Invoke(HttpContext context) {
            if (!context.WebSockets.IsWebSocketRequest) {
                return;
            }
            using (var socket = await context.WebSockets.AcceptWebSocketAsync()) {
                var client = new ChatClient(this.mux);
                await client.RunAsync(socket);
            }
        }


    }
}
