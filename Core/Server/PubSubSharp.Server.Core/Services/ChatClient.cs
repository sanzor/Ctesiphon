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
using System.Reactive.Linq;
using System.Reactive;
using
using System.IO;

namespace PubSubSharp.Server.Core {
    public sealed class ChatClient {

        private Task writeTask;

        private WebSocket socket;
        private RedisStore store;
        private IChannelSubscriptionService channelService;
        private ISubscriber sub;
        private ILogger log = Log.ForContext<ChatMessage>();
        private IObservable<ChatMessage> obs;
        private SemaphoreSlim @lock = new SemaphoreSlim(1);
        private const int RETRY = 3;


        private const double LCK_TIMEOUT = 1000;

        public ChatClient(WebSocket socket, RedisStore store, IChannelSubscriptionService channelService) {
            this.socket = socket;
            this.store = store;
            this.channelService = channelService;

        }


        public async Task RunAsync() {
            IEnumerable<IObservable<ChatMessage>> GetEndless(WebSocket ws) {
                Memory<byte> data = ArrayPool<byte>.Shared.Rent(1024);
                while (true) {
                    var obs = Observable.FromAsync<byte[]>(async () => {
                        var result = await ws.ReceiveAsync(data, CancellationToken.None);
                        var payload = data.Slice(0, result.Count);
                        return payload.ToArray();
                    }).Select(raw => {
                        var msg = raw.Decode<ChatMessage>();
                        return msg;
                    }).Repeat();
                    yield return obs;
                }
            }

            try {
                var endless = GetEndless(this.socket).Catch();
                

                this.sub = this.store.Connection.GetSubscriber();



                await writeTask;
            } catch (AggregateException ex) {
                var error = ex.GetBaseException();
                if (!(socket.State == WebSocketState.Closed)) {
                    await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, error.Message, cts.Token);
                }
            }
        }

        private async Task HandleMessageAsync(ChatMessage msg, CancellationToken token) {
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
                    await this.sub.SubscribeAsync(message.Channel, OnMessage);
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
        private void OnMessage(RedisChannel channel, RedisValue value) {
            log.Information($"Received:{value}\tfrom channel:{channel}");
            //this.queue.Add(value);
        }
        private async Task HandleUnsubscribeAsync(ChatMessage message) {
            await this.channelService.UnregisterChannelAsync(message.SenderID, message.Channel);
        }






    }
}
