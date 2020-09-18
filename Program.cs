using System;
using System.Collections;
using System.Threading;
using ENet;

namespace ENetTester
{
    class Program
    {
        static string serverAddress = string.Empty;
        static int port = 7777;
        static int maxChannels = 1;
        static int maxClients = 50;
        static bool killSwitch = false;
        static string dummyData = "Never gonna give you up. Never gonna let you down. Never gonna run around and desert you";

        static void Main(string[] args)
        {
            // Thread references
            Thread serverThread = new Thread(DoEnetServerWork);
            Thread clientThread = new Thread(DoEnetClientWork);

            // Is this server mode?
            bool isServer = false;

            // Advertisements
            Console.WriteLine("ENet Testing Application, written by Matt Coburn (SoftwareGuy/Coburn)");
            Console.WriteLine("Report bugs and submit PRs at https://github.com/SoftwareGuy/ENet-Tester");
            Console.WriteLine();
            Console.WriteLine("This application is licensed under the MIT license");
            Console.WriteLine("and comes with no warranty. Read the license file.");
            Console.WriteLine();

            Console.Write("Command Line: ");
            for (int i = 0; i < args.Length; i++)
            {
                Console.Write($"{args[i]} ");
            }
            Console.WriteLine();

            // Check args.
            if (args.Length > 1)
            {
                switch (args[0])
                {
                    case "client":
                        Console.WriteLine("CLIENT MODE ENABLED");
                        isServer = false;
                        break;

                    case "server":
                        Console.WriteLine("SERVER MODE ENABLED");
                        isServer = true;
                        break;

                    default:
                        PrintUsageInfo();
                        Environment.Exit(1);
                        break;
                }

                // Check if we've got an address
                if (args.Length >= 2)
                {
                    Console.WriteLine($"Using supplied address to connect/bind to: {args[1]}");
                    serverAddress = args[1];
                } else
                {
                    Console.WriteLine($"ERROR: No IP address specified!");
                    PrintUsageInfo();
                    Environment.Exit(1);
                }

                // Check if we've got an port
                if (args.Length == 3)
                {
                    Console.WriteLine($"Using supplied port to connect/bind to: {args[1]}");
                    if (int.TryParse(args[2], out int parsedPort))
                    {
                        if (parsedPort <= 0 || parsedPort > 65535)
                        {
                            Console.WriteLine($"WARN: Port out of range, using sane default of {port}.");
                        }
                        else
                        {
                            Console.WriteLine($"Using supplied address to connect/bind to: {args[2]}");
                            port = parsedPort;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"WARN: Port was bogus, using sane default of {port}.");
                    }
                }
                else
                {
                    Console.WriteLine($"WARN: Port was not supplied, using sane default of {port}.");
                }
            }
            else
            {
                PrintUsageInfo();
                Environment.Exit(1);
            }

            // ---
            Console.WriteLine();
            Console.WriteLine("At any time, press ESC to shutdown");
            Console.WriteLine();
            // ---

            // Initialize ENet
            Library.Initialize();

            if (isServer)
            {
                Console.WriteLine("Starting Server Thread...");
                serverThread.Start();
            }
            else
            {
                Console.WriteLine("Starting Client Thread...");
                clientThread.Start();
            }

            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                // Take a nap.
                ;
            }

            Console.WriteLine("Shutting down... this might take a moment...");
            killSwitch = true;

            if (isServer)
            {
                if (serverThread.IsAlive)
                {
                    serverThread.Join();
                }
            }
            else
            {
                if(clientThread.IsAlive)
                {
                    clientThread.Join();
                }
            }

            // Deinitialize ENet
            Library.Deinitialize();
            Environment.Exit(0);
        }

        static void PrintUsageInfo()
        {
            Console.WriteLine("ERROR: Bad usage of this application.");
            Console.WriteLine("Usage: ENetTester [server/client] [address] [port]");
            Console.WriteLine("Where:");
            Console.WriteLine("[server/client] refers to the mode you want. You need to specify either server or client.");
            Console.WriteLine("[address] is the IPv4/IPv6 address you wish to bind the server to or connect the client to.");
            Console.WriteLine("[port] is the port you wish to bind or connect to.");
            Console.WriteLine("");
        }


        // -- Threading stuff -- //
        private static void DoEnetServerWork()
        {
            Console.WriteLine("ENet Server Worker: Arrived");
            byte[] dummyDataArray = System.Text.Encoding.UTF8.GetBytes(dummyData);

            using (Host server = new Host())
            {
                Address address = new Address();

                address.Port = (ushort)port;
                server.Create(address, maxClients, maxChannels);

                Event netEvent;

                while (!killSwitch)
                {
                    bool polled = false;

                    while (!polled)
                    {
                        if (server.CheckEvents(out netEvent) <= 0)
                        {
                            if (server.Service(15, out netEvent) <= 0)
                                break;

                            polled = true;
                        }

                        switch (netEvent.Type)
                        {
                            case EventType.None:
                                break;

                            case EventType.Connect:
                                Console.WriteLine($"ENet Server Worker: Client connected - ID: {netEvent.Peer.ID}, IP: {netEvent.Peer.IP}");
                                break;

                            case EventType.Disconnect:
                                Console.WriteLine($"ENet Server Worker: Client disconnected - ID: {netEvent.Peer.ID}, IP: {netEvent.Peer.IP}");
                                break;

                            case EventType.Timeout:
                                Console.WriteLine($"ENet Server Worker: Client timeout - ID: {netEvent.Peer.ID}, IP: {netEvent.Peer.IP}");
                                break;

                            case EventType.Receive:
                                Console.WriteLine($"ENet Server Worker: Packet received from peer {netEvent.Peer.ID} ({netEvent.Peer.IP}:{netEvent.Peer.Port}); Ch. {netEvent.ChannelID}, {netEvent.Packet.Length} bytes");
                                netEvent.Packet.Dispose();

                                // Immediately counter the rick roll, with another rick roll packet.
                                Packet spam = default(Packet);
                                spam.Create(dummyDataArray, PacketFlags.Reliable);

                                int sendCode = netEvent.Peer.Send(0, ref spam);
                                
                                if (sendCode != 0)
                                {
                                    Console.WriteLine($"Send failure, ENet returned {sendCode}");
                                }

                                break;
                        }
                    }
                }

                server.Flush();
            }

            Console.WriteLine("ENet Server Worker: Departed");
        }

        private static void DoEnetClientWork()
        {            
            Console.WriteLine("ENet Client Worker: Arrived");
            int sendCode = -1;
            byte[] dummyDataArray = System.Text.Encoding.UTF8.GetBytes(dummyData);

            using (Host client = new Host())
            {
                Address address = new Address();

                address.SetHost(serverAddress);
                address.Port = (ushort)port;
                client.Create();

                Console.WriteLine("ENet Client Worker: Attempting connection.");
                Peer peer = client.Connect(address);

                Event netEvent;

                while (!killSwitch)
                {
                    bool polled = false;

                    while (!polled)
                    {
                        if (client.CheckEvents(out netEvent) <= 0)
                        {
                            if (client.Service(15, out netEvent) <= 0)
                                break;

                            polled = true;
                        }

                        switch (netEvent.Type)
                        {
                            case EventType.None:
                                break;

                            case EventType.Connect:
                                Console.WriteLine($"ENet Client Worker: Connected to server ({netEvent.Peer.IP}:{netEvent.Peer.Port})");

                                // Immediately send a rick roll.
                                Packet rickroll = default(Packet);
                                rickroll.Create(dummyDataArray, PacketFlags.Reliable);

                                sendCode = netEvent.Peer.Send(0, ref rickroll);

                                if (sendCode != 0)
                                {
                                    Console.WriteLine($"Send failure, ENet returned {sendCode}");
                                }
                                break;

                            case EventType.Disconnect:
                                Console.WriteLine("ENet Client Worker: Disconnected from server");
                                break;

                            case EventType.Timeout:
                                Console.WriteLine("ENet Client Worker: Connection timeout");
                                break;

                            case EventType.Receive:
                                Console.WriteLine($"ENet Client Worker: Packet received from server ({netEvent.Peer.IP}:{netEvent.Peer.Port}); Ch. {netEvent.ChannelID}, {netEvent.Packet.Length} bytes");
                                netEvent.Packet.Dispose();

                                // Uno reverse card
                                Packet spam = default(Packet);
                                spam.Create(dummyDataArray, PacketFlags.Reliable);

                                sendCode = peer.Send(0, ref spam);
                                if (sendCode != 0)
                                {
                                    Console.WriteLine($"Send failure, ENet returned {sendCode}");
                                }
                                break;
                        }
                    }
                }

                client.Flush();
                peer.DisconnectNow(0);
            }

            Console.WriteLine("ENet Client Worker: Departed");
        }
    }
}
