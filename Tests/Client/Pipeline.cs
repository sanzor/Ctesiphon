using PubSubSharp.Extensions;
using PubSubSharp.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Client {
    public class Pipeline {
        private ActionBlock<ChatMessage[]> actionBlock;
        private BufferBlock<byte[]> initBlock;
        public Pipeline() {
            CreatePipeline();
        }
        private void CreatePipeline() {

            this.initBlock = new BufferBlock<byte[]>(new DataflowBlockOptions { TaskScheduler = TaskScheduler.Default });
            ExecutionDataflowBlockOptions executionOptions = new ExecutionDataflowBlockOptions { TaskScheduler = TaskScheduler.Current };
            DataflowLinkOptions options = new DataflowLinkOptions { PropagateCompletion = true };
            TransformBlock<byte[], ChatMessage> tsfMessageBlock = new TransformBlock<byte[], ChatMessage>(y => {
                var message = Encoding.UTF8.GetString(y);
                var data = JsonSerializer.Deserialize<ChatMessage>(message);
                Console.WriteLine(y.ToJson());
                return data;
            }, executionOptions);

            BatchBlock<ChatMessage> batchBlock = new BatchBlock<ChatMessage>(10);


            this.actionBlock = new ActionBlock<ChatMessage[]>(y => {
                Console.WriteLine($"New batch at:{DateTime.Now.ToShortTimeString()}");
                foreach (var item in y) {
                    Trace.WriteLine($"{item.IssuedAt},{item.SenderID},{item.Value}");
                }
            });
            this.initBlock.LinkTo(tsfMessageBlock);
            tsfMessageBlock.LinkTo(batchBlock, options);
            batchBlock.LinkTo(this.actionBlock);

        }
        public void Post(byte[] message) {
            this.initBlock.Post(message);
        }
        public void Stop() {
            this.initBlock.Complete();
        }
    }
}
