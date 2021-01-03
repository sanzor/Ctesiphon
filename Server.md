# Introduction

This will be a multi-part series in which we are going to build from scratch a chat application . In this article we are going to build the chat server which is the backbone of the chat application using the following technologies:

- ASP NET Core
- Redis
- Websockets protocol

![](image/Server/1609216142929.png)

Supported Features:

- subscription to one or multiple chat rooms
- unsubscription from target/all chat rooms
- sending messages to target chat room
- receiving messages from all subscribed chat rooms

## Motivation

Ever since i started playing online games in middle-school back in 2003 (Warcraft 3) , i have been using messaging applications in order to communicate with my peers. The first such application  which in time became ubiquitous was Skype.

I have come to love it since it would enable me and  my friends to:

* send messages
* record audio
* share screen

Besides gaming ,we were also using it  for sharing school material(s) , homework discussions and why not ,  school gossip :D

Years after completely abandoning gaming and dabbling for some time in areas such as Industrial Automation , Embedded Devices i rediscovered my passion for chat apps , but this time i was poised to create them.

## General Mechanics

So lets say i am a user Adrian and i want to connect with my buddy , Vasi ,  and start exchanging messages. We will define all interactions between me and Vasi as belonging to a CHANNEL.

A typical flow would be the following:

- channel participant connects to server and subscribes to given channel
- participant starts sending messages to the server to target channel while also receiving incoming messages from target channel
- participant disconnects from the server and subsequentally from channel

Important thing to note is that , at any given time ,  there could be multiple such groups of people wanting to connect and communicate. Therefore , you can view the application as  a group of channels like in the image below:

![Channel Collection](image/Server/1609184427271.png)

Another important note is that there is nothing stopping a given user to subscribe to multiple channels.

As you can see from above , all messages sent by a channel participant will be **broadcasted** to **all** members of that channel , including the sender.

# Architecture

## Components

The proposed  solution will be composed of :

- **ASP NET Core Web application** - the server where our logic will run handling client operations (subscribe/unsubscribe/publish message/get channels)
- **Redis DB** serving as:
  - Data bus - we will be using the Publish/Subscribe functionality of Redis in order for clients to receive messages from subscribed channels.More on this can be found in the redis documentation [here](https://redis.io/topics/pubsub).
  - Storage medium , holding client data such as subscribed channel

## Communication Protocol

Since this is a chat application where the communication between a connected client and the given server is bidirectional (client sends messages , but also expects notifications from subscribed channel(s) ) ,  the protocol we will be using is **Websockets**.

## Flow

By flow we will be referring to the way both inbound- messages arriving from the client  and outbound messages  sent to the client are handled and where and how does the Websocket object fit in as well as the Redis database.

### Inbound Task

![Inbound Flow](image/Server/1609585970364.png)

The inbound task is basically a loop running in a  `System.Threading.Task` for those familiar with the `.NET` Ecosystem (an operation which is dispatched over the framework's  thread pool  , more on it [here](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-5.0)).

This task gets spawned at the begining of the session - when the user connects to the server via a upgradeable  http-to-websocket request .

#### Message types

Inside the inbound Task we will receive messages from the websocket connection and it is our responsibility to handle them accordingly.Therefore  the following messages have been defined:


| Message Type | Arguments | Action Performed |
| - | - | - |
| SUBSCRIBE | `Client ID`, `Channel` | Subscribes to Redis`Channel`  or sends back to client a SERVER_RESULT message with the failure reason ( already subscribed/ID mismatch) |
| UNSUBSCRIBE | `Channel` | Unsubscribes from Redis Channel or sends to client a SERVER_RESULT message with the reason for failure |
| MESSAGE | `ClientID`,<br />`Channel`,<br />`Payload` | Publishes`Payload` to target Redis  `Channel` on behalf of `Client ID` |
| GET_CHANNELS | `Client ID` | Retrieves all the channels that the`ClientID` is subscribed to. |

**Notes**:

- We did not include in the table the message of type`SERVER_RESULT` since this is an outbound message. The server sends this message to the client as the result of the attempted operation !
- The `SERVER_RESULT` messages , as you can see ,  are not written to the websocket , but to an Outbound Queue (this will be explained in the next section : *The Outbound Task* ) !

#### Subscribe/Unsubscribe mechanism

We are going to use an `ISubscriber` object provided by the `StackExchangeRedis` library in order to perform subscribe/unsubscribe operations on target channel(s).

To **subscribe** to a `Channel` we  will use the provided method

`ISubscriber.SubscribeAsync(RedisChannel channel, Action<RedisChannel,RedisValue> handler)` where :

- `channel` - the channel to which we want to subscribe
- `handler` -  a method that shall be triggered whenever a new message is available on the target channel.

To **unsubscribe** we use the provided method

`ISubscriber.UnsubscribeAsync(RedisChannel channel,Action<RedisChannel,RedisValue>handler)`  where :

- `channel` - the channel that we wish to stop receiving messages from
- `handler` - the method which was used to subscribe to the channel , (kept in a state variable)

An important note is that we need to store the `handler` as a state variable in order to subscribe/unsubscribe from any given channel.

### Outbound Task

![](image/Server/1609648304233.png)

The outbound task is a asynchronous task started from the  inbound task (during its inception).

As long as there are messages available in the queue we pop them  and send them to the client over the websocket connection.

When there are no messages inside the queue, the task blocks , awaiting new ones.

#### The Outbound Queue

The outbound queue acts as a sink for all producers as can be seen from the picture.In our case the producers are:

- **Inbound Task**: The messages that the server sends back to the client (messages of type SERVER_RESULT)
- **Redis**:  All messages that are published on channels on which our user is subscribed to.

# Implementation

## Prerequisites

Those familiar with setting up /using Redis and  .NET can skip this section.

### Installing Redis

For this solution you will need to install Redis Server . You can download it from [here](https://redis.io/download).

For windows users (me included) the easiest way to install redis is via the package manager *chocolatey* from  [here](https://chocolatey.org/install) . Once installed  from a terminal just run:

`choco redis`

If the install was successful from a terminal run

`redis-server`

and you should see the below output which indicates your redis server is up and running.

![](image/Server/1609517506383.png)

##### Using Redis-Cli

With the `redis-server` started  you can start playing with redis using the `Redis-Cli`from a terminal with the command`redis-cli`.

![](image/Server/1609672675137.png)

You can also test the `publish-subscribe` feature of redis by opening two `redis-cli` like below:

![](image/Server/1609672283904.png)

`redis-cli` can be used as a debugging/diagnosis tool , especially in our pubslish-subscribe scenario where you can easily hook up to a target channel and see if your messages get published/delivered.

### NET 5.0

For this application we are using .NET 5.0 and you can download it from  [here](https://dotnet.microsoft.com/download/dotnet/5.0).

## Source Code

We will be starting our project from a template of type `ASP NET Core Web Application`.

```
 public class Program {
        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            var webhostbuilder = WebHost.CreateDefaultBuilder(args)
                .UseUrls(Constants.SERVER_URL)
                .UseStartup<Startup>();
            return webhostbuilder;
        }
    }
```
