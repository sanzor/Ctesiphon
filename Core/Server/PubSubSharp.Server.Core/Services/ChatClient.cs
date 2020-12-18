using Serilog;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using PubSubSharp.Extensions;
using PubSubSharp.Models;
using System.Collections.Concurrent;
using System.Buffers;
using System.Threading;
using Serilog.Core;
using PubSubSharp.Interfaces;
using StackExchange.Redis;
using System.Text.Json;
using PubSub.Server.Core;

namespace PubSubSharp.Server.Core {
    public sealed class ChatClient {
        private  const int BUFFER_SIZE = 1024;


        private ILogger log = Log.ForContext<ChatClient>();
       
        private State state;
        private BlockingCollection<string> queue = new BlockingCollection<string>();


        private Action<RedisChannel, RedisValue> onRedisMessageHandler = null;
        public Action<RedisChannel, RedisValue> OnRedisMessageHandler {
            get {
                if (this.onRedisMessageHandler == null) {
                    this.onRedisMessageHandler= new Action<RedisChannel, RedisValue>((channel, value) => this.queue.Add(value));
                }
                return this.onRedisMessageHandler;
            }
        }
       
        public async Task RunAsync(WebSocket socket) {
            this.state.outboundTask = Task.Run(async () => {
                foreach (var item in this.queue.GetConsumingEnumerable()) {
                    var bytes = Encoding.UTF8.GetBytes(item);
                    await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            });
            await this.InboundLoopAsync(socket);
        }
      
        private async Task InboundLoopAsync(WebSocket socket, CancellationToken token = default) {

            byte[] inboundBuffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
            while (true) {
                token.ThrowIfCancellationRequested();
                try {
                    WebSocketReceiveResult wsResult = await socket.ReceiveAsync(inboundBuffer, token);
                    if (wsResult.MessageType == WebSocketMessageType.Close) {
                        ArrayPool<byte>.Shared.Return(inboundBuffer);
                        await this.CleanupSessionAsync();
                        return;
                    }
                    byte[] incomingBytes = inboundBuffer[0..wsResult.Count]; //{"Kind":3,"Payload":"{\"SenderId\":\"adrian\",\"Channel\":\"4\",\"Message\":\"dan\"}"}
                    WSMessage message = JsonSerializer.Deserialize<WSMessage>(Encoding.UTF8.GetString(incomingBytes));
                    await this.HandleMessageAsync(message);
                } catch (Exception ex) {
                    log.Error(ex.Message);
                } finally {

                }
            }
        }
        private async Task HandleMessageAsync(WSMessage message) {
            switch (message.Kind) {

                case WSMessage.DISCRIMINATOR.CLIENT__SUBSCRIBE:
                    ControlMessage subscribeMessage = JsonSerializer.Deserialize<ControlMessage>(message.Payload);
                    if (await state.redisDB.HashExistsAsync(this.state.ClientId, subscribeMessage.Channel)) {
                        queue.Add(new WSMessage { Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT, Payload = $"ALREADY SUBSCRIBED TO CHANNEL :{subscribeMessage.Channel}" }.ToJson());
                        return;
                    }
                    await this.state.subscriber.SubscribeAsync(subscribeMessage.Channel, this.onRedisMessageHandler);
                    await state.redisDB.HashSetAsync(subscribeMessage.ClientId, subscribeMessage.Channel, "set");
                    break;
                case WSMessage.DISCRIMINATOR.CLIENT_UNSUBSCRIBE:
                    ControlMessage unsubscribeMessage = JsonSerializer.Deserialize<ControlMessage>(message.Payload);
                    bool deleted = await state.redisDB.HashDeleteAsync(this.state.ClientId, unsubscribeMessage.Channel, CommandFlags.FireAndForget);
                    if (!await state.redisDB.HashDeleteAsync(this.state.ClientId, unsubscribeMessage.Channel, CommandFlags.FireAndForget)) {
                        queue.Add(new WSMessage { Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT, Payload = $" UNSUBSCRIBE UNSUCCESSFUL" }.ToJson());
                        return;
                    }
                    await this.state.subscriber.UnsubscribeAsync(unsubscribeMessage.Channel, this.onRedisMessageHandler);
                    queue.Add(new WSMessage { Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT, Payload = $" UNSUBSCRIBE SUCCESSFUL" }.ToJson());
                    break;
                case WSMessage.DISCRIMINATOR.CLIENT_MESSAGE:
                    ChatMessage chatMessage = JsonSerializer.Deserialize<ChatMessage>(message.Payload);
                    await this.state.subscriber.PublishAsync(chatMessage.Channel, $"Channel:{chatMessage.Channel},Sender:{chatMessage.SenderId},Message:{chatMessage.Message}");
                    break;
                case WSMessage.DISCRIMINATOR.CLIENT_GET_CHANNELS:
                    var channels = await this.state.redisDB.HashGetAllAsync(this.state.ClientId);
                    queue.Add(new WSMessage { Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT, Payload = channels.ToJson() }.ToJson() );
                    break;
            }
        }
        private async Task CleanupSessionAsync() {
            try {
                foreach (var channelHash in await this.state.redisDB.HashGetAllAsync(this.state.ClientId)) {
                    await this.state.subscriber.UnsubscribeAsync(channelHash.Name.ToString(), this.onRedisMessageHandler);
                }
            } catch (Exception ex) {
                log.Error(ex.Message);
                throw;
            }
            
        }
        public ChatClient(ConnectionMultiplexer mux) {

            this.state.subscriber = mux.GetSubscriber();
            this.state.redisDB = mux.GetDatabase();
        }



    }
}
