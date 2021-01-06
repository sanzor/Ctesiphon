using System.Text.Json;
using System.Threading.Tasks;

namespace PubSubSharp.Server {
    public  sealed  partial class ChatClient {
        
          // message  handling routine
        // SUBSCRIBE- adds channel to redis hashset
        // UNSUBSCRIBE- deletes channel from redis hashset
        // MESSAGE - publishes redis message to target channel
        // GET_CHANNELS - fetches all user subscribed channels from redis hashset
        private async Task HandleMessageAsync(WSMessage message) {
            switch (message.Kind) {

                case WSMessage.DISCRIMINATOR.CLIENT__SUBSCRIBE:
                    ControlMessage subscribeMessage = JsonSerializer.Deserialize<ControlMessage>(message.Payload);
                    if (subscribeMessage.ClientId != this.state.ClientId && this.state.ClientId != null) {
                        outboundQueue.Add(new WSMessage { 
                             Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT,
                             Payload = $"Error: ClientId mismatch ! " }
                        .ToJson());
                        return;
                    }
                    if (await state.redisDB.HashExistsAsync(this.state.ClientId = subscribeMessage.ClientId, subscribeMessage.Channel)) {
                        outboundQueue.Add(new WSMessage { 
                            Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT, 
                            Payload = $"Error: ALREADY SUBSCRIBED TO CHANNEL {subscribeMessage.Channel}"  }.ToJson());
                        return;
                    }
                    await this.state.subscriber.SubscribeAsync(subscribeMessage.Channel, this.OnRedisMessageHandler);
                    await state.redisDB.HashSetAsync(subscribeMessage.ClientId, subscribeMessage.Channel, "set");
                    outboundQueue.Add(new WSMessage { 
                        Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT,
                        Payload = $"Subscribed to channel :{subscribeMessage.Channel} SUCCESSFULLY !"}
                    .ToJson());
                    break;
                case WSMessage.DISCRIMINATOR.CLIENT_UNSUBSCRIBE:
                    ControlMessage unsubscribeMessage = JsonSerializer.Deserialize<ControlMessage>(message.Payload);
                    bool deleted = await state.redisDB.HashDeleteAsync(this.state.ClientId, unsubscribeMessage.Channel);
                    if (!deleted) {
                        outboundQueue.Add(new WSMessage { 
                            Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT, 
                            Payload = $" UNSUBSCRIBE UNSUCCESSFUL" }
                        .ToJson());
                        return;
                    }
                    await this.state.subscriber.UnsubscribeAsync(unsubscribeMessage.Channel, this.OnRedisMessageHandler);
                    outboundQueue.Add(new WSMessage { 
                        Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT, 
                        Payload = $" UNSUBSCRIBE SUCCESSFUL" }
                    .ToJson());
                    break;
                case WSMessage.DISCRIMINATOR.CLIENT_MESSAGE:
                    ChatMessage chatMessage = JsonSerializer.Deserialize<ChatMessage>(message.Payload);
                    if (!await this.state.redisDB.HashExistsAsync(chatMessage.ClientId, chatMessage.Channel)) {
                        outboundQueue.Add(new WSMessage {
                            Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT,
                            Payload = $"Can not send message.Client:{chatMessage.ClientId} " +
                            $"does not exist or is not subscribed to channel:{chatMessage.Channel}"}
                        .ToJson());
                    }
                    await this.state.subscriber.PublishAsync(chatMessage.Channel, $"Channel:{chatMessage.Channel},Sender:{chatMessage.ClientId},Message:{chatMessage.Message}");
                    break;
                case WSMessage.DISCRIMINATOR.CLIENT_GET_CHANNELS:
                    var channels = await this.state.redisDB.HashGetAllAsync(this.state.ClientId);
                    outboundQueue.Add(new WSMessage {
                         Kind = WSMessage.DISCRIMINATOR.SERVER__RESULT,
                         Payload = channels.ToJson()}
                    .ToJson());
                    break;
            }
        }

    }
}