using PubSubSharp.Models;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinformClient {
    class ChatClient : IChatClient {
        private ClientWebSocket connectedSocket;
        private CancellationTokenSource cts = new CancellationTokenSource();
        
        public ChatClient(ClientWebSocket connectedSocket) {
            this.connectedSocket = connectedSocket;
        }
        private ConcurrentHashSet<Channel> channels { get; set; } = new ConcurrentHashSet<Channel>();

        public IObservable<ChatMessage> GetObservable() {
            
        }

        public IEnumerable<Channel> GetRegisteredChannels() {
           return channels.GetAll();
        }

        public Task<bool> SubscribeAsync(string channel) {
            throw new NotImplementedException();
        }

        public Task<bool> UnSubscribeAsync(string channel) {
            throw new NotImplementedException();
        }
    }
}
