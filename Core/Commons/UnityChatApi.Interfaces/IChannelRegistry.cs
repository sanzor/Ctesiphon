using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityChatApi.Interfaces {
    public interface IChannelRegistry {
        Task<IEnumerable<string>> GetSubscribedChannelsAsync(string userId);
        Task<string> RegisterChannelAsync(string userId, string channelId);
        Task<string> UnregisterChannelAsync(string userId, string channelId);
    }
}
