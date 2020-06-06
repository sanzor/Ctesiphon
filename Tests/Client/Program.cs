using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Client {
    class Program {
        private const string URL = "ws://localhost:8300";

       
        static async Task Main(string[] args) {
            ClientWebSocket clientsocket = new ClientWebSocket();
            CancellationTokenSource loopCTS = new CancellationTokenSource();
            await clientsocket.ConnectAsync(new Uri(URL),CancellationToken.None);
            await Task.Run(async ()=>await )

        }
        private static async Task LoopAsync(ClientWebSocket socket) {
            while (true) {

            }
        }
    }
}
