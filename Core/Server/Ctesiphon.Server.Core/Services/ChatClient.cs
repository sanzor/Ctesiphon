using Serilog;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Ctesiphon.DataAccess;
using Ctesiphon.Extensions;
using Ctesiphon.Models;
using System.Collections.Concurrent;
using System.Buffers;
using System.Threading;
using Serilog.Core;
using Ctesiphon.Interfaces;
using StackExchange.Redis;
using static Ctesiphon.Models.ChatMessage;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;

using System.IO;
using System.Text.Json;

namespace Ctesiphon.Server.Core {
    public sealed class ChatClient {

        

        private WebSocket socket;
        private RedisStore store;
        private IChannelSubscriptionService channelService;
        private ISubscriber sub;
        private ILogger log = Log.ForContext<ChatMessage>();
        private BlockingCollection<ChatMessage> queue = new BlockingCollection<ChatMessage>();
        private SemaphoreSlim @lock = new SemaphoreSlim(1);
        private Task dequeueTask;
        private CancellationToken lifetimeToken;


        private const double LCK_TIMEOUT = 1000;

        public ChatClient(WebSocket socket, RedisStore store, IChannelSubscriptionService channelService,CancellationToken token) {
            this.socket = socket;
            this.store = store;
            this.channelService = channelService;
            this.lifetimeToken = token;

        }

        private async Task DequeueLoopAsync() {
            foreach (var item in this.queue.GetConsumingEnumerable()) {
                try {
                    lifetimeToken.ThrowIfCancellationRequested();
                    await this.socket.SendAsync(item.Encode(), WebSocketMessageType.Text, true, lifetimeToken);
                } catch (Exception) {
                    break;
                }
                
            }
        }
        public async Task RunAsync() {
            try {
                this.sub = this.store.Connection.GetSubscriber();
                await LoopAsync(lifetimeToken);
                this.dequeueTask = Task.Run(DequeueLoopAsync, this.lifetimeToken);

            } catch (AggregateException ex) {
                var error = ex.GetBaseException();
                if (!(socket.State == WebSocketState.Closed)) {
                    await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, error.Message,lifetimeToken);
                }
            }
        }
        private async Task LoopAsync(CancellationToken token) {
            Memory<byte> raw=ArrayPool<byte>.Shared.Rent(200);
            ValueWebSocketReceiveResult rez;
            while (socket.State.HasFlag(WebSocketState.Open)) {
                try {
                    token.ThrowIfCancellationRequested();
                    rez = await this.socket.ReceiveAsync(raw, token);
                } catch (Exception ex) {

                    break;
                }
                try {
                    var data = Encoding.UTF8.GetString(raw.Slice(0, rez.Count).ToArray());
                    var message = JsonSerializer.Deserialize<ChatMessage>(data);
                    await HandleSocketMessageAsync(message, token);
                } catch (Exception ex) {
                    continue;
                }
            }
            if(socket.State!=WebSocketState.Closed && socket.State != WebSocketState.Aborted) {
                try {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "socket closed", lifetimeToken);
                } catch (Exception ex) {

                   
                }
            }
            
        }

        private async Task HandleSocketMessageAsync(ChatMessage msg, CancellationToken token) {
            switch (msg.Kind) {
                case ChatMessage.DISCRIMINATOR.SUBSCRIBE:
                    await HandleSubscribeAsync(msg, token);
                    break;
                case ChatMessage.DISCRIMINATOR.UNSUBSCRIBE:
                    await this.HandleUnsubscribeAsync(msg);
                    break;
                case ChatMessage.DISCRIMINATOR.MESSAGE:
                    var sent = await this.sub.PublishAsync(msg.Channel, msg.ToJson());
                    break;
            }
        }
        private async Task HandleSubscribeAsync(ChatMessage message, CancellationToken token) {
            var result = await this.channelService.RegisterChannelAsync(message.SenderID, message.Channel);
            ChatMessage chatMsg = new ChatMessage { Channel = message.Channel, Kind = ChatMessage.DISCRIMINATOR.SERVER, SenderID = message.SenderID, Value = result };
            await @lock.WaitAsync(token);
            try {
                if (result == "Success") {
                    await this.sub.SubscribeAsync(message.Channel, HandleMessageAsync);
                }
            } catch (Exception ex) {
                log.Information($"Client:[{chatMsg.SenderID}] encountered error when subscribing.\tReason:{ex.Message}");
                chatMsg.Value = $"Could not subscribe\tReason:{ex.Message}";
            } finally {
                var mem = chatMsg.Encode();
                await this.socket.SendAsync(mem, WebSocketMessageType.Text, true, token);
                @lock.Release();
            }

        }
        private void HandleMessageAsync(RedisChannel channel, RedisValue value) {
            log.Information($"Received:{value}\tfrom channel:{channel}");
            try {
                var data = JsonSerializer.Deserialize<ChatMessage>(value);
                this.queue.Add(data);
            } catch (Exception ex) {

            }
           
        }
        private async Task HandleUnsubscribeAsync(ChatMessage message) {
            await this.channelService.UnregisterChannelAsync(message.SenderID, message.Channel);
        }






    }
}
