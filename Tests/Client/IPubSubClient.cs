using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    internal interface IPubSubClient {
        Task<string> SubscribeAsync(string channel);
        Task<string> UnsubscribeAsync(string channel);
        Task LoopAsync();
    }
}
