using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace PubSubSharp.Server {
    public static class EncodingExtensions {
      
        public static string ToJson<T>(this T obj) {
            var dat = JsonSerializer.Serialize(obj);
            return dat;
        }

    }
}
