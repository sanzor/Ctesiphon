using Ctesiphon.Extensions;
using Ctesiphon.Models;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace WinformClient {
    class SubscriptionProvider {
        private Dictionary<string, IObservable<ChatMessage>> subscribeMap = new Dictionary<string, IObservable<ChatMessage>>();
        private StreamSource source;
        private ClientWebSocket socket;
        public SubscriptionProvider(ClientWebSocket socket) {
            this.socket = socket;
        }
        public bool TryGetMessageStream(string channel,out IObservable<ChatMessage>messageStream) {
            if(!this.subscribeMap.TryGetValue(channel,out messageStream)) {
                return false;
            }
            return true;
        }
        public bool Subscribe(string channelName, string senderId="winformClient",CancellationToken token=default) {
            if(this.subscribeMap.TryGetValue(channelName,out IObservable<ChatMessage> observable)) {
                return false;
            }
            var subscribemessage = new ChatMessage { Kind = ChatMessage.DISCRIMINATOR.SUBSCRIBE, Channel = channelName, IssuedAt = DateTime.UtcNow, SenderID = senderId };
            this.socket.SendAsync(subscribemessage.Encode(), WebSocketMessageType.Text, true, token);

        }
        public bool Unsubscribe(string channelName,string senderId="winformClient") {
            if (!this.subscribeMap.TryGetValue(channelName, out IObservable<ChatMessage> observable)) {
                return false;
            }
            var subscribemessage = new ChatMessage { Kind = ChatMessage.DISCRIMINATOR.UNSUBSCRIBE, Channel = channelName, IssuedAt = DateTime.UtcNow, SenderID = senderId };
            this.socket.SendAsync(subscribemessage.Encode(), WebSocketMessageType.Text, true,
        }
    }
}
