using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;

namespace Observables {
    class Program {
        public static  async Task<string> ProduceAsync() {

            await Task.Delay(1);
            var k = Console.ReadKey();
            if (k.Key == ConsoleKey.A) {
                throw new Exception("Invalid key");
            }

            return k.Key.ToString();
            
        }
        public static IEnumerable<IObservable<string>> GetEndless() {
            for (; ; ) {
                var produceObs = Observable.FromAsync(ProduceAsync);
                var loop = produceObs.Repeat();
                yield return loop;
            }
        }
        private static EventWaitHandle handle = new EventWaitHandle(false,EventResetMode.AutoReset);
        
        static void Main(string[] args) {
            var endless=Observable.Catch(GetEndless());
            var sub = endless.SubscribeOn(NewThreadScheduler.Default).Subscribe(
                onNext: y => Console.WriteLine($"\n{DateTime.Now.ToShortDateString()}\t{y}"),
                onCompleted:()=>Console.WriteLine("done"),
                onError:ex=>Console.WriteLine($"{ex.Message}"));
          
          
            
            handle.WaitOne();
           
        }
    }
}
