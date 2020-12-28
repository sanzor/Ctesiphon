# Introduction

## Motivation

Ever since i started playing online games in middle-school back in 2003 (Warcraft 3) , i have been using messaging applications in order to communicate with my peers. The first such application  which in time became ubiquitous was Skype.

I have come to love it since it would enable me and  my friends to:

* send messages
* record audio
* share screen

thus , providing us a unique experience during long sessions of gaming.

Besides gaming ,we were also using it  for sharing school material(s) , homework discussions and why not school gossip :D

From then on i started to get a real fascination regarding chat apps , and pretty much any application that would support realtime notifications.

Years after completely abandoning gaming and dabbling for some time in areas such as Industrial Automation , Embedded Devices i rediscovered my passion for chat apps , but this time i wanted to create them.

## What is a chat application ?

So lets say i am a user Adrian and i want to connect with my buddy , Vasi and start exchanging messages. We will define all interactions between me and Vasi as belonging to a CHANNEL.

All messages sent by a participant will be **broadcasted** to **all** members of that channel. As you will see further on , the channel is the building block of our application.

Besides me and Vasi ,  at any given time there could be multiple such groups of people wanting to connect and communicate. Therefore , you can view the application as  a group of channels.

![Channel Collection](image/Server/1609184427271.png)



## Architecture

The chat server is basically the place where users plug in our system and start communicating with each other. For each new client the server will start a long running session ( *Thread* for old-school-ers or *Task*) that will end only upon user disconnection.

But what exactly would this communication mean?

Well this server is supposed to support the following operations:

- Channel subscription
