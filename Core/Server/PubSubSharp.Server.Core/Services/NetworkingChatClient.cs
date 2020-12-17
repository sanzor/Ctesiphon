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
    public sealed class NotWorkingChatClient {
        public class Wrapper {
           
            public Wrapper(WebSocket socket,int id) {
                this.socket = socket;
                this.Id = id;
            }
            public readonly WebSocket socket;
            public readonly int Id;
            public bool DidSubscribe;
        }
        private string CHANNEL = "xx";
        private ConnectionMultiplexer mux;
        private const int BUFFER_SIZE = 1024;
        private Wrapper state;
        private static int UniqueId;

        private ISubscriber subscriber;


        public NotWorkingChatClient(ConnectionMultiplexer mux) {
            this.mux = mux;
        }

        public async Task RunAsync(WebSocket socket) {
            //initialization
            this.subscriber = this.mux.GetSubscriber();
            byte[] inboundBuffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
            this.state = new Wrapper(socket,Interlocked.Increment(ref UniqueId));

            //printing session id

            await socket.SendAsync(Encoding.UTF8.GetBytes($"Current Session Id:{this.state.Id.ToString()}"), WebSocketMessageType.Text, true, CancellationToken.None);

            try {
                while (true) {
                    WebSocketReceiveResult wsResult = await socket.ReceiveAsync(inboundBuffer, CancellationToken.None);
                    if (wsResult.MessageType == WebSocketMessageType.Close) {
                        return;
                    }
                    byte[] incomingBytes = inboundBuffer[0..wsResult.Count];
                    string message = Encoding.UTF8.GetString(incomingBytes);
                    await this.HandleMessageAsync(message, socket);
                }
            } catch (Exception ex) { 
            } finally {
                await this.subscriber.UnsubscribeAsync(CHANNEL);
                await this.state.socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
        }
        private async Task HandleMessageAsync(string message, WebSocket socket) {
            if (!this.state.DidSubscribe) {
                Action<RedisChannel, RedisValue> f = async (channel, message) => {
                    Console.WriteLine($"incoming:{message},captured:{this.state.Id}");
                    byte[] bytes = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                };
                await this.subscriber.SubscribeAsync(CHANNEL,f);
                this.state.DidSubscribe = true;
            }
            await this.subscriber.PublishAsync(CHANNEL, this.state.Id);

        }



    }
}
