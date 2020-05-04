# PubSubSharp
Realtime web server for chat app featuring<br><br>
|<img src="https://codereviewvideos.com/blog/wp-content/uploads/2017/10/redis-logo.png" width=250 height=250/> |
<img src="https://miro.medium.com/max/1300/1*4BtGcPz3JauG9qsNXzLMXA.gif" width=450 height=300/> | 
<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://miro.medium.com/max/735/1*1oxw1WMb2loCAwVwTSgNjQ.jpeg"  width=500 height=200/><br>

     

## Scope<Br>

This is the API that will be powering a Unity3D chat client.The client will be able to connect to chatrooms and communicate with other 
users.

## Communication<br>

Since the chat client will exchange data with the server the preffered protocol of communication is via websockets.<br>
For each chat client , the server will open a websocket in order to facilitate message exchange.

## Chat Notifications <br>

The reactive side of the application  is done via  a Redis more specifically with its built-in  `pub-sub` functionality.
The chat client will subscribe to a channel(s) and send / receive data.
`Stackexchange.Redis` nuget is used for Redis communication.

## How it works <br>

For each connected client there will be :
 * A producer task - whenever the chat client sends a message to the server , it will be read from the websocket and published into the target channel provided by Redis.
 * An OnMessage handler - whenever a message is published on a target channel  , the subscribers of that channel  will receive it and Handle it in their own way ; in our case each
                         connected client will have a designated queue where messages from all channels will be pushed.
 * A consumer task - The purpose of this task is to pop messages from the aforementioned queue and write them over the websocket.If there are no messages in the queue
                     the task will wait (thanks to the `BlockingCollection` class provided by MSFT)
                     
![Schema](/Docs/Schema.png)
