using PubSubSharp.Conventions;
using PubSubSharp.Extensions;
using PubSubSharp.Models;
using Serilog;

using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
namespace Client {
    class Program {
        private const string URL = "ws://localhost:8600";
        private const string TEST_CHANNEL = "test";
        public static string ToCurrentAssemblyRootPath(string target) {
            var path = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().FullName).FullName, target);
            return path;
        }
        private void CreateLogger() {
            var logPath = ToCurrentAssemblyRootPath(Constants.LOG_FILE);
            Log.Logger = new LoggerConfiguration()
           .WriteTo.File(logPath, outputTemplate:Constants.LOG_OUTPUT_TEMPLATE)
           .WriteTo.ColoredConsole(outputTemplate: Constants.LOG_OUTPUT_TEMPLATE)
           .Enrich.FromLogContext()
           .CreateLogger();
        }
        static async Task Main(string[] args) {
           
            ClientWebSocket clientsocket = new ClientWebSocket();
            CancellationTokenSource loopCTS = new CancellationTokenSource();
            await clientsocket.ConnectAsync(new Uri(URL),CancellationToken.None);
            await clientsocket.SendAsync(new ChatMessage { SenderID = "Adisor", Kind = ChatMessage.DISCRIMINATOR.SUBSCRIBE, Channel = TEST_CHANNEL }.Encode(), WebSocketMessageType.Text, true, CancellationToken.None);
            PubSubClient client = new PubSubClient(clientsocket);

            try {
                Task t = Task.Run(async () => {
                    while (true) {
                        try {
                            loopCTS.Token.ThrowIfCancellationRequested();
                            Memory<byte> data = ArrayPool<byte>.Shared.Rent(1024);
                            var message=await clientsocket.ReceiveAndDecodeAsync<ChatMessage>(loopCTS.Token);
                            
                            
                            
                        } catch (Exception ex) {
                            if (!(clientsocket.State == WebSocketState.Closed)) {
                              
                            }
                           
                        }
                    }
                }, loopCTS.Token);
                while (!(Console.ReadKey().Key == ConsoleKey.Q)) ;
                loopCTS.Cancel();
            } catch (OperationCanceledException ex) {
                Log.Information("Cancel was issued");
            }catch(Exception ex) {
                Log.Error($"Closing due to error.\tReason:{ex.Message}");
            }
            
            

        }
        
    }
}
