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



        public static Task RunAsync(ClientWebSocket clientSocket) {
            PubSubClient client = new PubSubClient(clientSocket);
            return client.InnerRunAsync();
        }
        private Task InnerRunAsync() {
            this.loopCts = new CancellationTokenSource();
            logger.Information("Starting long running task");
            this.loopTask = Task.Run(async () => await LoopAsync(this.loopCts.Token), this.loopCts.Token);
        }


        private async Task LoopAsync(CancellationToken token) {
            var bufferBlock = new BufferBlock<byte[]>(new DataflowBlockOptions { BoundedCapacity = 10 });
            var toMessageBlock = new TransformBlock<byte[], ChatMessage>(x => {
                try {
                    var message=JsonSerializer.Deserialize<ChatMessage>(Encoding.UTF8.GetString(x));
                    return message;
                } catch (Exception ex) {

                    throw;
                }
            }
            , new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = ExecutionDataflowBlockOptions.Unbounded });
            var actionBlock = new ActionBlock<ChatMessage>(x => Console.WriteLine($"I received a message at channel:{x.Channel} of type:{x.Kind}"));

            bufferBlock.LinkTo(toMessageBlock);
            toMessageBlock.LinkTo(actionBlock);
            var memory = ArrayPool<byte>.Shared.Rent(1024);
            while (true) {
                if (token.IsCancellationRequested) {
                    break;
                }
                var message = await wsocket.ReceiveAsync(memory, CancellationToken.None);
                var data=memory.AsSpan().Slice(0, message.Count).ToArray();
                bufferBlock.Post(data);
               
            }
        }
        public PubSubClient(ClientWebSocket connectedClientSocket) {
            this.wsocket = connectedClientSocket;
        }

    }


}

