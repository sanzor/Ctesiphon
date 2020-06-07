using PubSubSharp.Extensions;
using PubSubSharp.Models;
using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
namespace Client {
    class Program {
        private const string URL = "ws://localhost:8600";
        private const string TEST_CHANNEL = "test";
       
        static async Task Main(string[] args) {
            ClientWebSocket clientsocket = new ClientWebSocket();
            CancellationTokenSource loopCTS = new CancellationTokenSource();
            await clientsocket.ConnectAsync(new Uri(URL),CancellationToken.None);
            await clientsocket.SendAsync(new ChatMessage { SenderID = "Adisor", Kind = ChatMessage.DISCRIMINATOR.SUBSCRIBE, Channel = TEST_CHANNEL }.Encode(), WebSocketMessageType.Text, true, CancellationToken.None);
            PubSubClient client = new PubSubClient(clientsocket);
           
           // client.Run();
            while (true) {
                try {
                    Memory<byte> data = ArrayPool<byte>.Shared.Rent(1024);
                    var s = await clientsocket.ReceiveAsync(data.ToArray(), CancellationToken.None);
                    var msg = data.Slice(0, s.Count);
                    var chat = msg.Decode<ChatMessage>();
                } catch (Exception ex) {
                    if (!(clientsocket.State == WebSocketState.Closed)) {
                        try {
                            await clientsocket.CloseAsync(WebSocketCloseStatus.NormalClosure, ex.Message, CancellationToken.None);
                        } catch (Exception exe) {

                            throw;
                        }
                        
                    }
                    throw;
                }
               
              
            }

        }
        
    }
}
