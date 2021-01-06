using Serilog;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Buffers;
using System.Threading;
using Serilog.Core;
using PubSubSharp;
using StackExchange.Redis;
using System.Text.Json;
using PubSub.Server;

namespace PubSubSharp.Server {
    public  sealed  partial class ChatClient {

        private const int BUFFER_SIZE = 1024;
        private State state = new State();
        private BlockingCollection<string> outboundQueue = new BlockingCollection<string>();
  
        private Action<RedisChannel, RedisValue> onRedisMessageHandler = null;
        public Action<RedisChannel, RedisValue> OnRedisMessageHandler {
            get {
                if (this.onRedisMessageHandler == null) {
                    this.onRedisMessageHandler = new Action<RedisChannel, RedisValue>
                                                ((channel, value)  => this.outboundQueue.Add(value));
                }
                return this.onRedisMessageHandler;
            }
        }  


        //Constructor -receives the multiplexer
        public ChatClient(ConnectionMultiplexer mux) {
            this.state.subscriber = mux.GetSubscriber();
            this.state.redisDB = mux.GetDatabase();
        }


        //entrypoint -starts asynchronous outbound task
        public async Task RunAsync(WebSocket socket) {
            this.state.outboundTask = Task.Run(async () => {
                foreach (var item in this.outboundQueue.GetConsumingEnumerable()) {
                    var bytes = Encoding.UTF8.GetBytes(item);
                    await socket.SendAsync(bytes,WebSocketMessageType.Text,true,CancellationToken.None);
                }
            });
            await this.InboundLoopAsync(socket);
        }
  


        // inbound task - receives messages ,parses them and handles them accordingly
        // on loop end - triggers the cleanup routine
        private async Task InboundLoopAsync(WebSocket socket) {

            byte[] inboundBuffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);   
            try {
                while (true) {
                    WebSocketReceiveResult wsResult = await socket.ReceiveAsync(inboundBuffer,CancellationToken.None);
                    if (wsResult.MessageType == WebSocketMessageType.Close) {
                        ArrayPool<byte>.Shared.Return(inboundBuffer);
                        return;
                    }
                    byte[] incomingBytes = inboundBuffer[0..wsResult.Count]; 
                    WSMessage message = JsonSerializer.Deserialize<WSMessage>(Encoding.UTF8.GetString(incomingBytes));
                    await this.HandleMessageAsync(message); 
                }
            } finally {
                await this.CleanupSessionAsync();
            }
        }

       // cleanup routine
       //cleans redis hashset containing subscribed channels && subscriptions to said channels
        private async Task CleanupSessionAsync() {
            foreach (var channelHash in await this.state.redisDB.HashGetAllAsync(this.state.ClientId)) {
                await this.state.subscriber.UnsubscribeAsync(channelHash.Name.ToString(), this.OnRedisMessageHandler);
            }
            await this.state.redisDB.KeyDeleteAsync(this.state.ClientId);
        }

  

      
    }
}
        