using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedisTest {
    class Program {
        public static string CHANNEL = "chan";
       
        public static int INITIAL_COUNT = 3;
        public static TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        public static TaskCompletionSource<bool> tcs2 = new TaskCompletionSource<bool>();


        static async Task Main(string[] args) {
            List<ISubscriber> subs = new List<ISubscriber>();
            
            ConnectionMultiplexer mux = ConnectionMultiplexer.Connect("localhost:6379");
            var sub1 = mux.GetSubscriber();
            var sub2 = mux.GetSubscriber();
            var h1 = new Action<RedisChannel, RedisValue>(async (chan, value) => {
                Console.WriteLine($"Handler1:{value}");
                if (value == "r" && tcs.Task.Status!=TaskStatus.RanToCompletion) {
                    tcs.SetResult(true);
                }
            });
            var h2 = new Action<RedisChannel, RedisValue>(async (chan, value) => {
                Console.WriteLine($"Handler2:{value}");
            });
            await sub1.SubscribeAsync(CHANNEL, h1);
            await sub2.SubscribeAsync(CHANNEL, h1);
            await sub1.SubscribeAsync(CHANNEL, h2);
            await tcs.Task;
            await sub1.UnsubscribeAsync(CHANNEL, h1);
            await tcs2.Task;
            Console.WriteLine("Hello World!");
        }

    }
}
