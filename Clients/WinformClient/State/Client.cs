using Ctesiphon.Extensions;
using Ctesiphon.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WinformClient {
    public class Client {
        
        public Config Config { get; }
        public ClientWebSocket socket { get; }
       
        public IObservable<ChatMessage> SourceStream { get; private set; }
        private Dictionary<string, IObservable<ChatMessage>> subscribeMap = new Dictionary<string, IObservable<ChatMessage>>();

        private void BuildSourceStream(CancellationToken token=default) {
            this.SourceStream = Observable.FromAsync(async () => {

                Memory<byte> buffer = ArrayPool<byte>.Shared.Rent(1024);
                var socketResult = await this.socket.ReceiveAsync(buffer, token);
                try {
                    var raw = Encoding.UTF8.GetString(buffer.Slice(0, socketResult.Count).ToArray());
                    var message = JsonSerializer.Deserialize<ChatMessage>(raw);
                    return message;
                } catch (Exception ex) {

                    return null;


                }
            }).Where(x => x != null).Repeat();
        }

        public async Task<bool> ConnectAsync(string uri, CancellationToken token = default) {
            if (this.socket.State == WebSocketState.Open) {
                return true;
            }
            if (this.socket.State == WebSocketState.Closed) {
                await DisconnectAsync();
            }
           
            try {
                
                await this.socket.ConnectAsync(new Uri(uri),token);
                bool state = this.socket.State == WebSocketState.Open;
              
                return this.socket.State == WebSocketState.Open;
            } catch (Exception ex) {

                return false;
            }


        }
        public async Task DisconnectAsync(CancellationToken token=default) {
            if (!(this.socket.State == WebSocketState.Closed) && this.socket.State != WebSocketState.Aborted) {
                try {
                   
                    await this.socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Socket closed", token);
                    
                } catch (Exception ex) {

                    return;
                }
            }
        }

        public bool Subscribe(string channelName, string senderId = "winformClient", CancellationToken token = default) {
            if (this.subscribeMap.TryGetValue(channelName, out IObservable<ChatMessage> observable)) {
                return false;
            }
            var subscribemessage = new ChatMessage { Kind = ChatMessage.DISCRIMINATOR.SUBSCRIBE, Channel = channelName, IssuedAt = DateTime.UtcNow, SenderID = senderId };
            this.socket.SendAsync(subscribemessage.Encode(), WebSocketMessageType.Text, true, token);

        }
        public bool Unsubscribe(string channelName, string senderId = "winformClient") {
            if (!this.subscribeMap.TryGetValue(channelName, out IObservable<ChatMessage> observable)) {
                return false;
            }
            var subscribemessage = new ChatMessage { Kind = ChatMessage.DISCRIMINATOR.UNSUBSCRIBE, Channel = channelName, IssuedAt = DateTime.UtcNow, SenderID = senderId };
            this.socket.SendAsync(subscribemessage.Encode(), WebSocketMessageType.Text, true,
        }

        public Client(Config config, ClientWebSocket socket) {
            this.socket = socket;

            this.Config = config;
        }

    }
}
