using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityChatApi.DataAccess;

namespace UnityChatApi.Server.Core {
    class ChannelHandlerService {
        private RedisStore store;
        public ChannelHandlerService(RedisStore store) {
            this.store = store;
        }
        public async Task<IEnumerable<string>> GetUserChannels(string userId) {

        }
        public async Task<bool> AddChannelAsync(string userId,string channel) {
            
        }
        public async Task<bool> DeleteChannelAsync(string userId,string channel) {

        }
    }
}
