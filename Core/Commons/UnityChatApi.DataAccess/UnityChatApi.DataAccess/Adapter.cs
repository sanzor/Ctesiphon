using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityChatApi.Models;

namespace UnityChatApi.DataAccess {
    public class Adapter {
        public static User Adapt(HashEntry[] entries) {
            var user = new User {
                Id = entries.First(x => x.Name == "id").Value,
            };
            return user;
        }
        public static HashEntry[] Adapt(User user) {
            List<HashEntry> entries = new List<HashEntry>();
            entries.Add(new HashEntry("id", user.Id));
            return entries.ToArray();
        }
    }
}
