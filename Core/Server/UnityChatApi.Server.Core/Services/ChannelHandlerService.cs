using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityChatApi.DataAccess;
using UnityChatApi.Interfaces;

namespace UnityChatApi.Server.Core {
    public class ChannelHandlerService:IChannelRegistry {
        private RedisStore store;
        public ChannelHandlerService(RedisStore store) {
            this.store = store;
        }
        public async Task<IEnumerable<string>> GetSubscribedChannelsAsync(string userId) {
            var list = await this.store.Database.HashGetAllAsync(userId);
            return list.Select(x => x.Value.ToString());
        }
        public async Task<string> RegisterChannelAsync(string userId, string channelId) {
            ITransaction tran = this.store.Database.CreateTransaction();
            var channels = await this.GetSubscribedChannelsAsync(userId);
            if (channels.Contains(channelId)) {
                return "Exists";
            }
            var didSet = this.store.Database.HashSetAsync(userId, channelId, DateTime.Now.ToString());
            var exec = await tran.ExecuteAsync();
            return (await didSet & exec) == true ? "Success" : "Failed";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async  Task<string> UnregisterChannelAsync(string userId, string channelId) {
            ITransaction tran = this.store.Database.CreateTransaction();
            var channels = await this.GetSubscribedChannelsAsync(userId);
            if (!channels.Contains(channelId)) {
                return "NotExists";
            }
            var deleted = this.store.Database.HashDeleteAsync(userId, channelId);
            var exec = await tran.ExecuteAsync();
            return (await deleted & exec) == true ? "Success" : "Failed";
        }
    }
}
