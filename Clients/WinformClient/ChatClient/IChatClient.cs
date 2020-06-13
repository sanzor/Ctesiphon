using PubSubSharp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WinformClient {
    public interface IChatClient {
        Task<bool> SubscribeAsync(string channel);
        Task<bool> UnSubscribeAsync(string channel);
        IEnumerable<Channel> GetRegisteredChannels();

        IObservable<ChatMessage> GetObservable();
    }
}
