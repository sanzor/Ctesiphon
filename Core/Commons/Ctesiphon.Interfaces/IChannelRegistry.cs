using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ctesiphon.Interfaces {
    public interface IChannelSubscriptionService {
        Task<IEnumerable<string>> GetSubscribedChannelsAsync(string userId);
        Task<string> RegisterChannelAsync(string userId, string channelId);
        Task<string> UnregisterChannelAsync(string userId, string channelId);
    }
}
