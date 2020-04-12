﻿using Microsoft.AspNetCore.Http;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityChatApi.DataAccess;
using UnityChatApi.Extensions;
using UnityChatApi.Server.Core;

namespace UnityChatApi.Server {
    public class SocketWare {
        private RequestDelegate next;
        private RedisStore store;
        public SocketWare(RequestDelegate _next,RedisStore store) {
            this.next = _next;
            this.store = store;
           
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
                var client = new ChatClient(socket, this.store);
                await client.RunAsync();
            } catch (Exception ex) {

                throw;
            }
            
        }
        

    }
}