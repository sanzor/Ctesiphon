using Serilog;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using UnityChatApi.DataAccess;
using UnityChatApi.Extensions;
using UnityChatApi.Models;
using StackExchange.Redis;

namespace UnityChatApi.Server.Core {
    public sealed class ChatClient {

        private Task writeTask;
        private Task popperTask;
        private WebSocket socket;
        private RedisStore store;
        private ISubscriber sub;
        public ChatClient(WebSocket socket, RedisStore store) {
            this.socket = socket;
            this.store = store;

        }
        public async Task RunAsync() {
            try {
                this.sub = this.store.Connection.GetSubscriber();
                this.writeTask = Task.Run(async () => await WriteLoopAsync());
                this.popperTask = Task.Run(async () => await PopLoopAsync());
                await Task.WhenAny(writeTask,popperTask);
            } catch (Exception ex) {

                throw;
            }

        }

        private async Task WriteLoopAsync() {

            while (true) {
                //receive some message from socket
                var message = await this.socket.ReceiveAsync([....]); 
                //find list of subscribed channels and if it does not exist subscribe to it
                //publish message to target channel
                await HandleMessageAsync(message);
            }

        }
        private async Task PopLoopAsync() {
            
            while (true) {
                //pop a message from the queue that is filled by channel delegates
                var data = await this.store.Connection.BLPOP("popList");
                //send the message on the websocket
                this.socket.SendAsync(data);
            }
        }
        private async Task HandleMessageAsync(Message msg) {
            //subscribe to channel if it is not already subscribed;
            await this.sub.SubscribeAsync(msg.Channel, async (channel, value) => {
                await this.store.Database.ListRightPushAsync("popList", msg.Value);
            });
            await this.sub.PublishAsync(msg.Channel, msg.Value);
        }
    }
}
