using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using CommunicationLibrary;
using CommunicationLibrary.Events.Server;
using CommunicationLibrary.Models;
using log4net;
using log4net.Config;

namespace ConsoleChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            
            var ip = GetIpAddress();
            Console.WriteLine($"Using IP-Address: {ip}");

            var port = 0;
            while (!(port > 1024 && port < 65536))
            {
                Console.WriteLine("Please insert Port on which the server shall listen (1025-65535):");
                var portString = Console.ReadLine();
                port = int.Parse(portString ?? "1024");
            }

            var server = new ChatServer(ip, port);
            server.ServerStart += ServerStartHandler;
            server.ServerStop += ServerStopHandler;
            server.UserJoin += UserJoinHandler;
            server.UserLeave += UserLeaveHandler;
            server.ReceivedMessage += ReceivedMessageHandler;
            server.ReceivedMessageTo += ReceivedMessageToHandler;
            
            server.StartServer();

            var command = "";
            while (!command.ToLower().Equals("exit"))
            {
                command = Console.ReadLine() ?? "";
            }
            
            server.StopServer();
            Environment.Exit(0);
        }

        private static void ReceivedMessageHandler(object sender, ReceivedMessageEventArgs e)
        {
            WriteTimedMessage($"'{e.Sender.Name}' sent the message '{e.Message}'");
        }
        
        private static void ReceivedMessageToHandler(object sender, ReceivedMessageToEventArgs e)
        {
            WriteTimedMessage($"'{e.Sender.Name}' sent to '{e.Recipient.Name}' the message '{e.Message}'");
        }

        private static void UserLeaveHandler(object sender, User e)
        {
            WriteTimedMessage($"User '{e.Name}' left the server.");
        }

        private static void UserJoinHandler(object sender, User e)
        {
            WriteTimedMessage($"User '{e.Name}' joined the server.");
        }

        private static void ServerStopHandler(object sender, EventArgs e)
        {
            WriteTimedMessage("Server stopped.");
        }

        private static void WriteTimedMessage(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}");
        }

        private static void ServerStartHandler(object sender, EventArgs e)
        {
            WriteTimedMessage("Server started.");
        }

        private static string GetIpAddress()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as IPEndPoint;
            
            return endPoint?.Address.ToString();
        }
    }
}