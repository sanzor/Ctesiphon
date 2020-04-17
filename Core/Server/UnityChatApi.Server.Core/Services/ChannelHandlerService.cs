using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var list =await this.store.Database.ListRangeAsync($"user:{userId}_channels");
            return list.Select(x=>x.ToString());
        }
        public async Task<bool> AddChannelAsync(string userId,string channel) {
            
            ITransaction tran = this.store.Database.CreateTransaction();
            var channels = await this.GetUserChannels(userId);
            if (channels.Contains(channel)) {
                return false;
            }
            var push=this.store.Database.ListLeftPushAsync($"user:{userId}_channels", channel);
            var exec = await tran.ExecuteAsync();
            return exec;
            
        }
        public async Task<bool> DeleteChannelAsync(string userId,string channel) {
            ITransaction tran = this.store.Database.CreateTransaction();
            var channels = await this.GetUserChannels(userId);
            if (!channels.Contains(channel)) {
                return false;
            }
            var push = this.store.Database.ListLeftPopAsync($"user:{userId}_channels");
            var exec = await tran.ExecuteAsync();
            return exec;
        }
    }
}
