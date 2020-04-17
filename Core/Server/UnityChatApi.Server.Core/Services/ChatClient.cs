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
using Serilog.Core;

namespace UnityChatApi.Server.Core {
    public sealed class ChatClient {

        private Task writeTask;
        private Task popperTask;
        private WebSocket socket;
        private RedisStore store;
        private ISubscriber sub;
        private ILogger log = Log.ForContext<ChatMessage>();
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
                await Task.WhenAll(writeTask, popperTask);
            } catch (Exception ex) {
                throw;
            }

        }
        private async Task WriteLoopAsync() {

            while (true) {
                //receive some message from socket
                var message = await this.socket.ReceiveAndDecode<ChatMessage>();

                //find list of subscribed channels and if it does not exist subscribe to it
                //publish message to target channel
                await HandleMessageAsync(message);
            }

        }
        private async Task HandleMessageAsync(ChatMessage msg) {

            switch (msg.Kind) {

                case ChatMessage.DISCRIMINATOR.SUBSCRIBE: await this.sub.SubscribeAsync(msg.Channel, OnMessage); break;
                case ChatMessage.DISCRIMINATOR.UNSUBSCRIBE: this.sub.Unsubscribe(msg.Channel, OnUnsubscribe); break;
                case ChatMessage.DISCRIMINATOR.MESSAGE: var sent = await this.sub.PublishAsync(msg.Channel, msg.ToJson()); break;
                default: throw new NotSupportedException();

            }
        }

        private void OnMessage(RedisChannel channel, RedisValue value) {
            log.Information($"Received:{value}\tfrom channel:{channel}");
            this.queue.Add(value);
        }
        private void OnUnsubscribe(RedisChannel channel, RedisValue value) {
            log.Information($"Ending subscription to channel:{channel}");
        }
        private async Task PopLoopAsync() {
            try {
                while (true) {
                    //pop a message from the queue that is filled by channel delegates
                    var data = this.queue.Take();
                    var bytes = Encoding.UTF8.GetBytes(data);
                    //send the message on the websocket
                    await this.socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
                }

            } catch (Exception ex) {

                throw;
            }
            //mb user cancellation token on socket
           
        }



    }
}
