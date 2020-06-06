using Serilog;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using PubSubSharp.DataAccess;
using PubSubSharp.Extensions;
using PubSubSharp.Models;
using System.Collections.Concurrent;
using System.Buffers;
using System.Threading;
using Serilog.Core;
using PubSubSharp.Interfaces;
using StackExchange.Redis;

namespace PubSubSharp.Server.Core {
    public sealed class ChatClient {

        private Task writeTask;
        private Task popperTask;
        private WebSocket socket;
        private RedisStore store;
        private IChannelRegistry channelRegistryService;
        private ISubscriber sub;
        private ILogger log = Log.ForContext<ChatMessage>();
        private BlockingCollection<RedisValue> queue = new BlockingCollection<RedisValue>();
        private ReaderWriterLockSlim @lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private const int LCK_TIMEOUT = 1000;
        public ChatClient(WebSocket socket, RedisStore store,IChannelRegistry channelRegistry) {
            this.socket = socket;
            this.store = store;
            this.channelRegistryService = channelRegistry;
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
            try {
                while (true) {
                    var message = await this.socket.ReceiveAndDecode<ChatMessage>();
                    await HandleMessageAsync(message);
                }
            } catch (Exception ex) {

                throw;
            }

           

        }
        private async Task HandleMessageAsync(ChatMessage msg) {

            switch (msg.Kind) {
                case ChatMessage.DISCRIMINATOR.SUBSCRIBE: await HandleSubscribeAsync(sub,msg); break;
                case ChatMessage.DISCRIMINATOR.UNSUBSCRIBE: this.sub.Unsubscribe(msg.Channel, OnUnsubscribe); break;
                case ChatMessage.DISCRIMINATOR.MESSAGE: var sent = await this.sub.PublishAsync(msg.Channel, msg.ToJson()); break;
                default: throw new NotSupportedException();
            }
        }
        private async Task HandleSubscribeAsync(ISubscriber sub,ChatMessage message) {
            var result = await this.channelRegistryService.RegisterChannelAsync(message.SenderID, message.Channel);

            ChatMessage chatMsg = new ChatMessage { Channel = message.Channel, Kind = ChatMessage.DISCRIMINATOR.SERVER, SenderID = message.SenderID, Value = result };
            @lock.TryEnterWriteLock(LCK_TIMEOUT);
            try {
                if (result == "Success") {
                    await this.sub.SubscribeAsync(message.Channel,OnMessage);
                }
            } catch (Exception ex) {
                chatMsg.Value = "Could not subscribe";
            } finally {
                await this.socket.SendAsync(chatMsg.Encode(), WebSocketMessageType.Text, true, CancellationToken.None);
                @lock.ExitWriteLock();
            }
            
        }
        private async Task HandleUnsubscribeAsync(ISubscriber sub,ChatMessage message) {

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
        }



    }
}
