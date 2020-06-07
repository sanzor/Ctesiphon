using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace PubSubSharp.Extensions {
    public static class EncodingExtensions {
        public static async Task<T> ReceiveAndDecode<T>(this WebSocket socket,CancellationToken token, int bufferSize = 1024) {
            byte[] rawInput = ArrayPool<byte>.Shared.Rent(bufferSize);
            var rec = await socket.ReceiveAsync(rawInput, token);
            var result = rawInput.AsMemory().Slice(0, rec.Count).Decode<T>();
            ArrayPool<byte>.Shared.Return(rawInput);
            return result;
        }

        public static string MakeId() {
            return Guid.NewGuid().ToString();
        }
        public static T Decode<T>(this byte[] data) {
            string rawString = Encoding.UTF8.GetString(data);
            T result = JsonSerializer.Deserialize<T>(rawString);
            return result;
        }
        public static T Decode<T>(this Memory<byte> payload) {
            var result = Decode<T>(payload.ToArray());
            return result;
        }
        public static ReadOnlyMemory<byte> Encode<T>(this T data) {
            string rawString = JsonSerializer.Serialize(data);
            ReadOnlyMemory<byte> payload = Encoding.UTF8.GetBytes(rawString);
            return payload;
        }
        public static string ToJson<T>(this T obj) {
            var dat = JsonSerializer.Serialize(obj);
            return dat;
        }
        public static T FromJson<T>(this string data) {
            T dat = JsonSerializer.Deserialize<T>(data);
            return dat;
        }


    }
}
