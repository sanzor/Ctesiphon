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
        private SemaphoreSlim @lock = new SemaphoreSlim(1);
        
        private const double LCK_TIMEOUT = 1000;

        public ChatClient(WebSocket socket, RedisStore store, IChannelSubscriptionService channelService) {
            this.socket = socket;
            this.store = store;
            this.channelService = channelService;
        }

        public async Task RunAsync() {
            CancellationTokenSource cts = new CancellationTokenSource();
            try {
                this.sub = this.store.Connection.GetSubscriber();
                this.writeTask = Task.Run(async () => await ReceiveLoopAsync(cts.Token), cts.Token);
                this.popperTask = Task.Run(async () => await PopLoopAsync(cts.Token), cts.Token);
                await Task.WhenAll(writeTask, popperTask);
            } catch (AggregateException ex) {
                var error = ex.GetBaseException();
                if (!(socket.State == WebSocketState.Closed)) {
                    await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, error.Message, cts.Token);
                }
            }
        }
        private async Task PopLoopAsync(CancellationToken token) {
            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(token)) {
                linked.CancelAfter(TimeSpan.FromSeconds(15));
                while (true) {
                    try {
                        token.ThrowIfCancellationRequested();
                        var data = this.queue.Take();
                        var bytes = Encoding.UTF8.GetBytes(data);
                        await this.socket.SendAsync(data, WebSocketMessageType.Text, true, token);
                    } catch (TaskCanceledException cancelledExc) {
                        log.Error($"Cancel was issued");
                        break;
                    } catch (Exception ex) {
                        log.Error($"Error\tReason:{ex.Message}");
                        break;
                    }
                }
            }
              
        }
        private async Task ReceiveLoopAsync(CancellationToken token) {
            
          
            ChatMessage message = null;
            var str = new ChatMessage().ToJson();
            while (true) {
                try {
                    token.ThrowIfCancellationRequested();
                    using (var linked = CancellationTokenSource.CreateLinkedTokenSource(token)) {
                       // linked.CancelAfter(TimeSpan.FromSeconds(30));
                        message = await this.socket.ReceiveAndDecodeAsync<ChatMessage>(linked.Token);
                    }

                } catch (OperationCanceledException ex) {
                    log.Information($"Cancel was issued");
                } catch (Exception ex) {
                    log.Error($"Error in write loop.\tReason{ex.Message}");
                    throw;
                }
                try {
                    await HandleMessageAsync(message,token);
                } catch (Exception ex) {
                    if (new[] { DISCRIMINATOR.SUBSCRIBE, DISCRIMINATOR.UNSUBSCRIBE }.Contains(message.Kind)) {
                        log.Error($"User:{message.SenderID} could not:{message.Kind}\tReason:{ex.Message}");
                        throw;
                    }

                }

            }
        }

        private async Task HandleMessageAsync(ChatMessage msg,CancellationToken token) {
            switch (msg.Kind) {
                case ChatMessage.DISCRIMINATOR.SUBSCRIBE: await HandleSubscribeAsync(sub, msg,token); break;
                case ChatMessage.DISCRIMINATOR.UNSUBSCRIBE: await this.HandleUnsubscribeAsync(sub, msg); break;
                case ChatMessage.DISCRIMINATOR.MESSAGE: var sent = await this.sub.PublishAsync(msg.Channel, msg.ToJson()); break;
            }
        }
        private async Task HandleSubscribeAsync(ISubscriber sub, ChatMessage message,CancellationToken token) {
            var result = await this.channelService.RegisterChannelAsync(message.SenderID, message.Channel);
            ChatMessage chatMsg = new ChatMessage { Channel = message.Channel, Kind = ChatMessage.DISCRIMINATOR.SERVER, SenderID = message.SenderID, Value = result };
            await @lock.WaitAsync(token);
            try {
                if (result == "Success") {
                    await this.sub.SubscribeAsync(message.Channel, OnMessage);
                }
            } catch (Exception ex) {
                chatMsg.Value = $"Could not subscribe\tReason:{ex.Message}";
            } finally {
                var mem = chatMsg.Encode();
                await this.socket.SendAsync(mem, WebSocketMessageType.Text, true, token);
                @lock.Release();
            }

        }
        private void OnMessage(RedisChannel channel, RedisValue value) {
            log.Information($"Received:{value}\tfrom channel:{channel}");
            this.queue.Add(value);
        }
        private async Task HandleUnsubscribeAsync(ISubscriber sub, ChatMessage message) {
            await this.channelService.UnregisterChannelAsync(message.SenderID, message.Channel);
        }
        





    }
}
