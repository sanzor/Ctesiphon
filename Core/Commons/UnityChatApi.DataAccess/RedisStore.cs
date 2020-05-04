using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PubSub.DataAccess {
    public sealed class RedisStore {
        public ConnectionMultiplexer Connection;
        public RedisStore(string connectionString) {
            this.Connection = ConnectionMultiplexer.Connect(connectionString);
        }
        public IDatabase Database => this.Connection.GetDatabase();
    }
}
