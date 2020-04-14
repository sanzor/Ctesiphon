using Serilog;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using UnityChatApi.DataAccess;
using UnityChatApi.Extensions;
using UnityChatApi.Models;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Buffers;
using System.Threading;

namespace UnityChatApi.Server.Core {
    public sealed class ChatClient {

        private Task writeTask;
        private Task popperTask;
        private WebSocket socket;
        private RedisStore store;
        private ISubscriber sub;
        private BlockingCollection<RedisValue> queue = new BlockingCollection<RedisValue>();
        
        public ChatClient(WebSocket socket, RedisStore store) {
            this.socket = socket;
            this.store = store;
        }

        public async Task RunAsync() {
            try {
                this.sub = this.store.Connection.GetSubscriber();
                this.writeTask = Task.Run(async () => await WriteLoopAsync());
                this.popperTask = Task.Run(async () => await PopLoopAsync());
                await Task.WhenAny(writeTask,popperTask);
            } catch (Exception ex) {
                throw;
            }

        }

        private async Task HandleMessageAsync(Message msg) {

            //subscribe to channel if it is not already subscribed;
            await this.sub.SubscribeAsync(msg.Channel, (channel, value) => {
                this.queue.Add(value);
             });
            await this.sub.PublishAsync(msg.Channel, msg.Value);
        }

        private async Task WriteLoopAsync() {
            var buffer=ArrayPool<byte>.Shared.Rent(1024);
            while (true) {
                //receive some message from socket
                var result = await this.socket.ReceiveAsync(buffer, CancellationToken.None);
                //find list of subscribed channels and if it does not exist subscribe to it
                //publish message to target channel
                await HandleMessageAsync(message);
            }

        }
        private async Task PopLoopAsync() {
            
            while (true) {
                //pop a message from the queue that is filled by channel delegates
                var data =this.queue.Take();
                //send the message on the websocket
                await this.socket.SendAsync(data.decode);
            }
        }
       
     
       
    }
}
