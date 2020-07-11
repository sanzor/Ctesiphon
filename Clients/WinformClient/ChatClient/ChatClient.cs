using Ctesiphon.Models;
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
        private IObservable<ChatMessage> mainObservable;
        private Task mainLoop;
        
        public ChatClient(ClientWebSocket connectedSocket) {
            this.connectedSocket = connectedSocket;
            //this.mainLoop=Task.Run()
        }
        private ConcurrentHashSet<Channel> channels { get; set; } = new ConcurrentHashSet<Channel>();

        
        private async Task RunLoop(CancellationToken token) {
            try {
                while (true) {
                    token.ThrowIfCancellationRequested();
                    //var message = await connectedSocket.re
                   
                }
            } catch (Exception) {

                throw;
            }
           
        }
        public IEnumerable<Channel> GetRegisteredChannels() {
           return channels.GetAll();
        }

        public Task<bool> TrySubscribeAsync(string channelName,out Channel channel) {
            throw new NotImplementedException();
        }

        public Task<bool> UnSubscribeAsync(string channel) {
            throw new NotImplementedException();
        }

      
        public IObservable<ChatMessage> GetSocketObservable() {
            return this.mainObservable;
        }
    }
}
