using Serilog;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PubSubSharp.Extensions;
using PubSubSharp.Models;
using System.Threading.Tasks.Dataflow;
using System.Buffers;
using System.Text.Json;
namespace Client {
    public class PubSubClient {
        private readonly ClientWebSocket wsocket;
        private CancellationTokenSource loopCts;
        private ILogger logger = Log.ForContext<PubSubClient>();
        private Task loopTask;
      
        

       
        private void InnerRun() {
            this.loopCts = new CancellationTokenSource();
            logger.Information("Starting long running task");
            this.loopTask = Task.Run(async () => await LoopAsync(this.loopCts.Token), this.loopCts.Token);
           
        }
        public void Run() {
            this.InnerRun();
        }
        public void Stop() {
            this.loopCts.Cancel();
        }

        private async Task LoopAsync(CancellationToken token) {
            //Pipeline pipeline = new Pipeline();
            Memory<byte> buffer = ArrayPool<byte>.Shared.Rent(1024);
            while (true) {
                if (token.IsCancellationRequested) {
                   // pipeline.Stop();
                    break;
                }
                try {
                    var result = await wsocket.ReceiveAsync(buffer, CancellationToken.None);
                    var data = buffer.Slice(0, result.Count).ToArray();
                    var value = data.Decode<ChatMessage>();
                    Console.WriteLine($"{value.ToJson()}");
                } catch (Exception ex) {
                    Log.Error($"Could not process message.\tReason:{ex.Message}\t Trying the next one");
                    if (wsocket.State == WebSocketState.Aborted) {
                        Log.Information("Closing the program , socket was closed");
                        break;
                    }
                   
                }
                
               // pipeline.Post(data);
               
            }
        }
        public PubSubClient(ClientWebSocket connectedClientSocket) {
            this.wsocket = connectedClientSocket;
        }

    }


}

