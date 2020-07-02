using PubSubSharp.Models;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace WinformClient {
    public class State {
        public Config Config { get; }
        public ClientWebSocket Socket { get; }
        public IObservable<ChatMessage> MessagePipe { get; set; }
        public State(Config config,ClientWebSocket socket) {
            this.Socket = socket;
            this.Config = config;
        }
        
    }
}
