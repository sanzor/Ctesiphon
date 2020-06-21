using PubSubSharp.Conventions;
using PubSubSharp.Extensions;
using PubSubSharp.Models;
using Serilog;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
namespace Client {
    class Program {
        private const string URL = "ws://localhost:8600";
        private const string TEST_CHANNEL = "test";
        private static EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.AutoReset);
        public static string ToCurrentAssemblyRootPath(string target) {
            var path = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().FullName).FullName, target);
            return path;
        }
        private void CreateLogger() {
            var logPath = ToCurrentAssemblyRootPath(Constants.LOG_FILE);
            Log.Logger = new LoggerConfiguration()
           .WriteTo.File(logPath, outputTemplate: Constants.LOG_OUTPUT_TEMPLATE)
           .WriteTo.ColoredConsole(outputTemplate: Constants.LOG_OUTPUT_TEMPLATE)
           .Enrich.FromLogContext()
           .CreateLogger();
        }
        static async Task Main(string[] args) {
            IEnumerable<IObservable<ChatMessage>> GetFactory(ClientWebSocket socket){
                for (; ; ) {
                    var obb = Observable.FromAsync(async (cts) => {
                        Memory<byte> data = ArrayPool<byte>.Shared.Rent(1024);
                        return await socket.ReceiveAndDecodeAsync<ChatMessage>(CancellationToken.None);
                    });
                    var loop = obb.Repeat();
                    yield return loop;
                }
            }
            ClientWebSocket clientsocket = new ClientWebSocket();
            CancellationTokenSource loopCTS = new CancellationTokenSource();
            await clientsocket.ConnectAsync(new Uri(URL), CancellationToken.None);
            await clientsocket.SendAsync(new ChatMessage { SenderID = "Adisor", Kind = ChatMessage.DISCRIMINATOR.SUBSCRIBE, Channel = TEST_CHANNEL }.Encode(), WebSocketMessageType.Text, true, CancellationToken.None);
            PubSubClient client = new PubSubClient(clientsocket);


            var endless = GetFactory(clientsocket).Catch();

            endless.Subscribe(
                onNext: x => {
                    var t = 2 + 1;
                    Process.Start(new ProcessStartInfo { FileName = "werl.exe" ,Arguments=$"-sname {new Random().Next(0,100)}_{x.Channel} -setcookie {x.Channel}"});
                    
                },
                onCompleted: () => Console.WriteLine("done"),
                onError: z => Console.WriteLine($"{z.Message}"),
                token:loopCTS.Token
                );

            handle.WaitOne();


        }

    }
}

