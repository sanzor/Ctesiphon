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
using static PubSubSharp.Models.ChatMessage;
using System.Linq;

namespace PubSubSharp.Server.Core {
    public sealed class ChatClient {

        private Task writeTask;
        private Task popperTask;
        private WebSocket socket;
        private RedisStore store;
        private IChannelSubscriptionService channelService;
        private ISubscriber sub;
        private ILogger log = Log.ForContext<ChatMessage>();

        private BlockingCollection<RedisValue> queue = new BlockingCollection<RedisValue>();
        private ReaderWriterLockSlim @lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private CancellationTokenSource cts = new CancellationTokenSource();
        private const int LCK_TIMEOUT = 1000;

        public ChatClient(WebSocket socket, RedisStore store, IChannelSubscriptionService channelService) {
            this.socket = socket;
            this.store = store;
            this.channelService = channelService;
        }

        public async Task RunAsync() {
            try {
                this.sub = this.store.Connection.GetSubscriber();
                this.writeTask = Task.Run(async () => await ReceiveLoopAsync(cts.Token), cts.Token);
                this.popperTask = Task.Run(async () => await PopLoopAsync(cts.Token), cts.Token);
                await Task.WhenAll(writeTask, popperTask);
            } catch (AggregateException ex) {

            }
        }
        private async Task PopLoopAsync(CancellationToken token) {

            while (true) {
                try {
                    if (token.IsCancellationRequested) {
                        break;
                    }
                    var data = this.queue.Take();
                    var bytes = Encoding.UTF8.GetBytes(data);
                    await this.socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
                } catch (Exception ex) {
                    log.Error($"Error\tReason:{ex.Message}");
                }
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken token) {
            ChatMessage message = null;
            var str = new ChatMessage().ToJson();
            while (true) {
                try {
                    if (token.IsCancellationRequested) {
                        break;
                    }
                    message = await this.socket.ReceiveAndDecode<ChatMessage>();
                   
                } catch (Exception ex) {
                    log.Error($"Error in write loop:{ex.Message}");
                    if (!(socket.State == WebSocketState.Closed)) {
                        var errorMessage = $"Closing socket for client:{message.SenderID}\tReason:{ex.Message}";
                        log.Debug(errorMessage);
                        await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, errorMessage, CancellationToken.None);
                    }
                    throw;
                }
                try {
                    await HandleMessageAsync(message);
                } catch (Exception ex) {
                    if(new[] { DISCRIMINATOR.SUBSCRIBE, DISCRIMINATOR.UNSUBSCRIBE }.Contains(message.Kind)) {
                        log.Error($"User:{message.SenderID} could not:{message.Kind}\tReason:{ex.Message}");
                        throw;
                    }
                    
                }

            }
        }

        private async Task HandleMessageAsync(ChatMessage msg) {
            switch (msg.Kind) {
                case ChatMessage.DISCRIMINATOR.SUBSCRIBE: await HandleSubscribeAsync(sub, msg); break;
                case ChatMessage.DISCRIMINATOR.UNSUBSCRIBE: await this.HandleUnsubscribeAsync(sub, msg); break;
                case ChatMessage.DISCRIMINATOR.MESSAGE: var sent = await this.sub.PublishAsync(msg.Channel, msg.ToJson()); break;
            }
        }
        private async Task HandleSubscribeAsync(ISubscriber sub, ChatMessage message) {
            var result = await this.channelService.RegisterChannelAsync(message.SenderID, message.Channel);
            ChatMessage chatMsg = new ChatMessage { Channel = message.Channel, Kind = ChatMessage.DISCRIMINATOR.SERVER, SenderID = message.SenderID, Value = result };
            @lock.TryEnterWriteLock(LCK_TIMEOUT);
            try {
                if (result == "Success") {
                    await this.sub.SubscribeAsync(message.Channel, OnMessage);
                }
            } catch (Exception ex) {
                chatMsg.Value = $"Could not subscribe\tReason:{ex.Message}";
            } finally {
                await this.socket.SendAsync(chatMsg.Encode(), WebSocketMessageType.Text, true, CancellationToken.None);
                if (@lock.IsWriteLockHeld) {
                    @lock.ExitWriteLock();
                }
            }

        }
        private async Task HandleUnsubscribeAsync(ISubscriber sub, ChatMessage message) {
            await this.channelService.UnregisterChannelAsync(message.SenderID, message.Channel);
        }
        private void OnMessage(RedisChannel channel, RedisValue value) {
            log.Information($"Received:{value}\tfrom channel:{channel}");
            this.queue.Add(value);
        }





    }
}
