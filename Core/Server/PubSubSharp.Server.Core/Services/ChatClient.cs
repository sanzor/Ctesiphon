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

        private ILogger log = Log.ForContext<WSMessage>();
        private BlockingCollection<string> queue = new BlockingCollection<string>();
       


      
        private RedisStore redisStore;
        private const int BUFFER_SIZE = 1024;

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
        private async Task InboundLoopAsync(WebSocket socket,CancellationToken token = default) {

            Task redisTask = Task.Run(async () => {
                foreach (var item in this.queue.GetConsumingEnumerable()) {
                    token.ThrowIfCancellationRequested();
                    byte[] bytes = Encoding.UTF8.GetBytes(item);
                    await socket.SendAsync(bytes, WebSocketMessageType.Text, true, token);
                }
            });

            byte[] inboundBuffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
            while (true) {
                token.ThrowIfCancellationRequested();
                try {
                    WebSocketReceiveResult wsResult = await socket.ReceiveAsync(inboundBuffer, token);
                    if (wsResult.MessageType == WebSocketMessageType.Close) {
                        ArrayPool<byte>.Shared.Return(inboundBuffer);
                        return;
                    }
                    byte[] incomingBytes = inboundBuffer[0..wsResult.Count]; //{"kind":0,"payload":"{\"senderId\":\"adrian\",\"channel\":\"4\",\"message\":\"hello\"}"}
                    WSMessage message = JsonSerializer.Deserialize<WSMessage>(Encoding.UTF8.GetString(incomingBytes));
                    await this.HandleMessageAsync(message);
                } catch (Exception ex) {
                    log.Error(ex.Message);
                }
            }
        }
        private async Task HandleMessageAsync(WSMessage message) {
            switch (message.Kind) {
                case WSMessage.DISCRIMINATOR.SUBSCRIBE:
                    ControlMessage subscribeMessage = JsonSerializer.Deserialize<ControlMessage>(message.Payload);
                    await this.redisSubscriber.UnsubscribeAsync(subscribeMessage.Channel);
                    await this.redisSubscriber.SubscribeAsync(subscribeMessage.Channel, (channel, message) => this.queue.Add(message));
                    break;
                case WSMessage.DISCRIMINATOR.UNSUBSCRIBE:
                    ControlMessage unsubscribeMessage = JsonSerializer.Deserialize<ControlMessage>(message.Payload);
                    await this.redisSubscriber.UnsubscribeAsync(unsubscribeMessage.Channel);
                    break;
                case WSMessage.DISCRIMINATOR.MESSAGE:
                    ChatMessage chatMessage = JsonSerializer.Deserialize<ChatMessage>(message.Payload);
                    await this.redisSubscriber.PublishAsync(chatMessage.Channel, $"{chatMessage.SenderId}:{chatMessage.Message}");
                    break;
            }
        }



    }
}
