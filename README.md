# ENet-Tester

[![Ko-Fi](https://img.shields.io/badge/Donate-Ko--Fi-red)](https://ko-fi.com/coburn) 
[![PayPal](https://img.shields.io/badge/Donate-PayPal-blue)](https://paypal.me/coburn64)
![MIT Licensed](https://img.shields.io/badge/license-MIT-green.svg)

## What is this?
ENet-Tester is a NET Core-based Testing Application for native ENet libraries. 
It is used internally to identify if [a fork of ENet-CSharp](http://github.com/SoftwareGuy/ENet-CSharp) was failing to operate correctly.

The testing application is crude, rushed and pretty jank, however it sends a endless loop of the first few lines of *Never Gonna Give You Up* by Rick Ansley back and forth.
This was deemed sufficient to ensure that the sending pipeline of ENet was functioning correctly. Right now the application is hard-coded to only allow up to 50 clients out of
the 4096 theortical maximum that ENet supports - this is to save ENet from looping over empty Peer slots.

## How to use?

**Server Mode** spawns a ENet Reliable UDP server instance, listening for client connections on an IP address and port that you specify.

**Client Mode** spawns a ENet client connection to the server IP address and port specified.

To use, you need to download the repository, and compile the application. Once compiled, invoke the application executable like so:

```
ENetTester.exe [server/client] [ip] [port]
```
**Make sure** you slap a release of the ENet for your architecture from, for example, my [ENet-CSharp releases](https://github.com/SoftwareGuy/ENet-CSharp/releases)
next to the application. Or **you will** get an "DllNotFound" exception.

### Server and Client Connection Examples

If you want a server on 192.168.88.200 and port 8008, you would do:
```
ENetTester.exe server 192.168.88.200 8008
```

If you wanted a client to connect to a existing instance on 192.168.88.220 port 1337, you would do:
```
ENetTester.exe client 192.168.88.220 1337
```

If you fail to provide the needed information, such as server or client mode and IP Address, the application will not start and instead will show some
helpful usage information like so:

```
ENet Testing Application
Report bugs and submit PRs at https://github.com/SoftwareGuy/ENet-Tester

This application is licensed under the MIT license
and comes with no warranty. Read the license file.

Command Line:
ERROR: Bad usage of this application.
Usage: ENetTester [server/client] [address] [port]
Where:
[server/client] refers to the mode you want. You need to specify either server or client.
[address] is the IPv4/IPv6 address you wish to bind the server to or connect the client to.
[port] is the port you wish to bind or connect to.
```

## Todo List

-	User-configurable peer count
-	Better argument on the commandline parsing
-	Less console "received packet" spam, maybe duration, packets recieved in and sent out, total bytes
-	Less rickroll

## I have a problem.

Get a solution by opening a Issue ticket. Be verbose about your problem though and provide info like your operating system, how it broke, etc. 
If you don't do this then I will not be able to give a timely reply to your ticket. Help me help you by providing all the information I need.

## License
Released under MIT license, read LICENSE.md.