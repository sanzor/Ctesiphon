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
using System.Text.Json;


namespace PubSubSharp.Server.Core {
    public sealed class ChatClient {
        public class SocketWrapper {
            private static int CurrentValue;
            public SocketWrapper(WebSocket socket) {
                this.socket = socket;
                this.Id = Interlocked.Increment(ref CurrentValue);
            }
            public readonly WebSocket socket;
            public readonly int Id;
            public bool DidSubscribe;
        }
        private ILogger log = Log.ForContext<WSMessage>();
        private BlockingCollection<string> queue = new BlockingCollection<string>();





        private RedisStore redisStore;
        private const int BUFFER_SIZE = 1024;
        private SocketWrapper state;

        private ISubscriber redisSubscriber;


        public ChatClient(RedisStore store) {

            this.redisStore = store;
            this.redisSubscriber = store.Connection.GetSubscriber();
        }

        public async Task RunAsync(WebSocket socket) {
            try {
                await this.InboundLoopAsync(socket);
            } catch (OperationCanceledException ex) {
                log.Information("Task was cancelled");
            } catch (Exception ex) {
                log.Error($"{ex.Message}");
            } finally {
                await this.redisSubscriber?.UnsubscribeAllAsync();
            }

        }
        private async Task InboundLoopAsync(WebSocket socket, CancellationToken token = default) {
            this.state = new SocketWrapper(socket);

            await socket.SendAsync(Encoding.UTF8.GetBytes($"New Id:{this.state.Id.ToString()}"), WebSocketMessageType.Text, true, CancellationToken.None);
            byte[] inboundBuffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
            while (true) {
                token.ThrowIfCancellationRequested();
                try {
                    WebSocketReceiveResult wsResult = await socket.ReceiveAsync(inboundBuffer, token);
                    if (wsResult.MessageType == WebSocketMessageType.Close) {
                        ArrayPool<byte>.Shared.Return(inboundBuffer);
                        await this.redisSubscriber.UnsubscribeAllAsync();
                        return;
                    }
                    byte[] incomingBytes = inboundBuffer[0..wsResult.Count]; //{"Kind":3,"Payload":"{\"SenderId\":\"adrian\",\"Channel\":\"4\",\"Message\":\"dan\"}"}
                    WSMessage message = JsonSerializer.Deserialize<WSMessage>(Encoding.UTF8.GetString(incomingBytes));
                    await this.HandleMessageAsync(message, socket);
                } catch (Exception ex) {
                    log.Error(ex.Message);
                }
            }
        }
        private async Task HandleMessageAsync(WSMessage message, WebSocket socket) {
            switch (message.Kind) {

                case WSMessage.DISCRIMINATOR.SUBSCRIBE:
                    ControlMessage subscribeMessage = JsonSerializer.Deserialize<ControlMessage>(message.Payload);
                    await this.redisSubscriber.UnsubscribeAsync(subscribeMessage.Channel);
                    this.redisSubscriber.Subscribe(subscribeMessage.Channel, async (channel, message) => {
                        byte[] bytes = Encoding.UTF8.GetBytes(message);
                        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                    });
                    break;
                case WSMessage.DISCRIMINATOR.UNSUBSCRIBE:
                    ControlMessage unsubscribeMessage = JsonSerializer.Deserialize<ControlMessage>(message.Payload);
                    await this.redisSubscriber.UnsubscribeAsync(unsubscribeMessage.Channel);
                    //this.queue.Add(unsubscribeMessage.ToJson());
                    break;
                case WSMessage.DISCRIMINATOR.MESSAGE:
                    ChatMessage chatMessage = JsonSerializer.Deserialize<ChatMessage>(message.Payload);
                    await this.redisSubscriber.PublishAsync(chatMessage.Channel, $"Channel:{chatMessage.Channel},Sender:{chatMessage.SenderId},Message:{chatMessage.Message}");
                    break;
            }
        }

    

    }
}
