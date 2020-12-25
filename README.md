# Lockstep Multiplayer Test
Simple test implementation of a lockstep multiplayer game using LiteNetLib

Start with "server" argument to act as server, otherwise the app will act as client.

* Start server
* Connect one or more clients
* Start game
* Click in the area to move player

![Demo](https://github.com/Skleni/lockstep-multiplayer/raw/master/multiplayer.gif)

## Docker instructions for Hole Punch Server:

`docker build --tag hold-punch-server`
`docker run -p 5463:5463/udp hole-punch-server`