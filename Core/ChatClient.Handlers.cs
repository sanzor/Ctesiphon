using System.Text.Json;
using System.Threading.Tasks;

namespace PubSubSharp.Server {
    public sealed partial class ChatClient {

        // message  handling routine
        // SUBSCRIBE- adds channel to redis hashset
        // UNSUBSCRIBE- deletes channel from redis hashset
        // MESSAGE - publishes redis message to target channel
        // GET_CHANNELS - fetches all user subscribed channels from redis hashset
        private async Task HandleMessageAsync(WSMessage message) {
            switch (message.Kind) {

                case WSMessage.DISCRIMINATOR.CLIENT__SUBSCRIBE:
                    ControlMessage subscribeMessage = JsonSerializer.Deserialize<ControlMessage>(message.Payload);
                    await this.HandleSubscribeAsync(subscribeMessage);
                    break;
                case WSMessage.DISCRIMINATOR.CLIENT_UNSUBSCRIBE:
                    ControlMessage unsubscribeMessage = JsonSerializer.Deserialize<ControlMessage>(message.Payload);
                    await this.HandleUnsubscribeAsync(unsubscribeMessage);
                    break;
                case WSMessage.DISCRIMINATOR.CLIENT_MESSAGE:
                    ChatMessage chatMessage = JsonSerializer.Deserialize<ChatMessage>(message.Payload);
                    await this.HandleMessageAsync(chatMessage);
                    break;
                case WSMessage.DISCRIMINATOR.CLIENT_GET_CHANNELS:
                    await this.HandleGetChannelsAsync(message);
                    break;
            }
        }
        private async Task HandleSubscribeAsync(ControlMessage subscribeMessage) {
            WSMessage outboundMessage = null;
            if (subscribeMessage.ClientId != this.state.ClientId && this.state.ClientId != null) {
                outboundMessage = new WSMessage {
                    Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT,
                    Payload = $"Error: ClientId mismatch ! "
                };
                outboundQueue.Add(outboundMessage.ToJson());
                return;
            }
            if (await state.redisDB.HashExistsAsync(this.state.ClientId = subscribeMessage.ClientId, subscribeMessage.Channel)) {
                outboundMessage= new WSMessage {
                    Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT,
                    Payload = $"Error: ALREADY SUBSCRIBED TO CHANNEL {subscribeMessage.Channel}"
                };
                outboundQueue.Add(outboundMessage.ToJson());
                return;
            }
            await this.state.subscriber.SubscribeAsync(subscribeMessage.Channel, this.OnRedisMessageHandler);
            await state.redisDB.HashSetAsync(subscribeMessage.ClientId, subscribeMessage.Channel, "set");
            outboundMessage = new WSMessage {
                Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT,
                Payload = $"Subscribed to channel : {subscribeMessage.Channel} SUCCESSFULLY !"
            };
            outboundQueue.Add(outboundMessage.ToJson());
        }
        private async Task HandleUnsubscribeAsync(ControlMessage unsubscribeMessage) {
            WSMessage outboundMessage = null;
            bool deleted = await state.redisDB.HashDeleteAsync(this.state.ClientId, unsubscribeMessage.Channel);
            if (!deleted) {
                outboundMessage = new WSMessage {Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT,Payload = $" UNSUBSCRIBE UNSUCCESSFUL"};
                outboundQueue.Add(outboundMessage.ToJson());
                return;
            }
            await this.state.subscriber.UnsubscribeAsync(unsubscribeMessage.Channel, this.OnRedisMessageHandler);
            outboundMessage = new WSMessage {
                Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT,
                Payload = $" UNSUBSCRIBE SUCCESSFUL"
            };
            outboundQueue.Add(outboundMessage.ToJson());
        }
        private async Task HandleMessageAsync(ChatMessage chatMessage) {
            if (!await this.state.redisDB.HashExistsAsync(chatMessage.ClientId, chatMessage.Channel)) {
                WSMessage outboundMessage = new WSMessage {
                    Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT,
                    Payload = $"Can not send message. Client : {chatMessage.ClientId} " +
                    $"does not exist or is not subscribed to channel : {chatMessage.Channel}"
                };
                outboundQueue.Add(outboundMessage.ToJson());
            }
            await this.state.subscriber.PublishAsync(chatMessage.Channel, $"Channel : {chatMessage.Channel}, Sender : {chatMessage.ClientId}, Message : {chatMessage.Message}");
        }
        private async Task HandleGetChannelsAsync(WSMessage message) {
            var channels = await this.state.redisDB.HashGetAllAsync(this.state.ClientId);
            outboundQueue.Add(new WSMessage {
                Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT,
                Payload = channels.ToJson()
            }
            .ToJson());
        }
    }
}