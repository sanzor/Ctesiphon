# PubSubSharp
Realtime web server for chat app featuring<br><br>
<img src="https://codereviewvideos.com/blog/wp-content/uploads/2017/10/redis-logo.png" width=250 height=250/> |
<img src="https://i0.wp.com/josephmuciraexclusives.com/wp-content/uploads/2019/12/WebSockets.png?resize=1200%2C882&ssl=1" width=300 height=250/> <br>
<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://miro.medium.com/max/735/1*1oxw1WMb2loCAwVwTSgNjQ.jpeg"  width=530 height=200/><br>

     

## What it is and what it does <Br>

This is a `ASP .NET Core 3.1` realtime server suitable for powering up IoT,Games,Collaboration or Messaging apps.

## Prerequisites
 - [**Redis**](https://redis.io)  will be used for its [`pub-sub`](https://redis.io/topics/pubsub) functionality.More details can be found [here](https://redis.io/) 
 - [Simple WebSocket Client](https://chrome.google.com/webstore/detail/simple-websocket-client/pfdhoblngboilpfeibdedpjgfnlcodoo) I prefer to use it for testing purposes , before you can actually hook a real `websocket` client.

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
