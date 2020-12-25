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
        private ILogger logger = Log.ForContext<SocketWare>();
        public SocketWare(RequestDelegate _next, ConnectionMultiplexer mux) {
            this.next = _next;
            this.mux = mux;
        }
        public async Task Invoke(HttpContext context) {
            if (!context.WebSockets.IsWebSocketRequest) {
                return;
            }
            try {
                using (var socket = await context.WebSockets.AcceptWebSocketAsync()) {
                    var client = new ChatClient(this.mux);
                    await client.RunAsync(socket);
                }
            } catch (Exception ex) {
                logger.Error($"Error:{ex.Message}");

            }


        }


    }
}
