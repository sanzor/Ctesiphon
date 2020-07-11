using Ctesiphon.Models;
using Ctesiphon.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WinformClient {
    public interface IChatClient {
       
        Task<bool> TrySubscribeAsync(string channelName,out Channel channel);
        IEnumerable<Channel> GetRegisteredChannels();

        IObservable<ChatMessage> GetSocketObservable();
    }
}
