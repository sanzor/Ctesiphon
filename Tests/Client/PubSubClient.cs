using Serilog;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client {
    public class PubSubClient {
        private readonly ClientWebSocket wsocket;
        private CancellationTokenSource loopCts;
        private ILogger logger = Log.ForContext<PubSubClient>();
        private Task loopTask;

        public static Task RunAsync(ClientWebSocket clientSocket) {
            PubSubClient client = new PubSubClient(clientSocket);
            return client.InnerRunAsync();
        }
        private Task InnerRunAsync() {
            this.loopCts = new CancellationTokenSource();
            logger.Information("Starting long running task");
            this.loopTask = Task.Run(async() => await LoopAsync(this.loopCts.Token),this.loopCts.Token);
        }


        private async Task LoopAsync(CancellationToken token) {
            
            while (true) {
                if (token.IsCancellationRequested) {
                    break;
                }
            }
        }
        public PubSubClient(ClientWebSocket connectedClientSocket) {
            this.wsocket = connectedClientSocket;
        }

    }
}
