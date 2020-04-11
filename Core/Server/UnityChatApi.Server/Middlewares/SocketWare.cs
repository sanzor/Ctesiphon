using Microsoft.AspNetCore.Http;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityChatApi.Extensions;
using UnityChatApi.Models;

namespace UnityChatApi.Server {
    public class SocketWare {
        private RequestDelegate next;
        public SocketWare(RequestDelegate _next) {
            this.next = _next;
        }
        public async Task Invoke(HttpContext context) {
            if (!context.WebSockets.IsWebSocketRequest) {
                return;
            }
            var socket=await context.WebSockets.AcceptWebSocketAsync();
            await LoopAsync(socket);
        }
        private async Task LoopAsync(WebSocket socket) {
            Task sendTask = Task.Run(async () => await SendAsync(socket));
            Task receiveTask = Task.Run(async () => await ReceiveAsync(socket));
            var finish=await Task.WhenAny(sendTask, receiveTask);
        }
        private async Task SendAsync(WebSocket socket) {
            ReadOnlyMemory<byte> data = Encoding.UTF8.GetBytes($"From server at time:\t{DateTime.Now.ToShortDateString()}\t Sugi pwla");
            try {
                while (true) {
                    await Task.Delay(1000);
                    await socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            } catch (Exception ex) {

                throw;
            }
            
        }
        private async Task ReceiveAsync(WebSocket socket) {
            var buffer=ArrayPool<byte>.Shared.Rent(1024);
            try {
                while (true) {
                    Message x = await socket.ReceiveAndDecode<Message>();
                    Console.WriteLine($"Date:{x.IssuedAt.ToShortDateString()}\tValue:{x.Value}");
                }
            } catch (Exception ex) {
                ArrayPool<byte>.Shared.Return(buffer);
                throw;
            }
            
        }

    }
}
