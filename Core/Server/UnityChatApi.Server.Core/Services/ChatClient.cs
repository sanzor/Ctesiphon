using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace UnityChatApi.Server.Core {
    class ChatClient {
        private Task readTask;
        private Task writeTask;
        private WebSocket socket;
        public ChatClient(WebSocket socket) {
            this.socket = socket;
        }
        public async Task RunAsync() {
            this.readTask = Task.Run(async () => {

            });
        }
        private async Task ReadAsync() {

        }
        private async Task WriteAsync() {

        }
    }
}
