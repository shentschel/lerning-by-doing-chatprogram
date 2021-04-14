using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CommunicationLibrary;
using CommunicationLibrary.Events.Client;
using CommunicationLibrary.Models;
using log4net;
using log4net.Config;

namespace ConsoleChatClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            
            Console.WriteLine("Please enter Server IP and Port (Format: 'IP:Port'):");
            var ipPort = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ipPort))
            {
                Console.WriteLine("No IP:Port defined.");
                Environment.Exit(-1);
            }

            var connectionDetails = ipPort.Split(':');
            var client = new ChatClient(connectionDetails[0], int.Parse(connectionDetails[1]));

            client.IncomingMessage += IncomingMessageHandler;
            client.ServerConnect += ServerConnectHandler;
            client.ServerDisconnect += ServerDisconnectHandler;
            client.ServerJoin += ServerJoinHandler;
            client.UserJoin += UserJoinHandler;
            client.UserLeave += UserLeaveHandler;
            client.NewUserList += NewUserListHandler;
            client.UserKick += UserKickHandler;
            
            Console.WriteLine("Please insert name");
            var userName = Console.ReadLine();
            var color = "#00000000";
            
            client.JoinServer(userName, color);

            var message = "";
            while (!message.ToLower().Equals("exit"))
            {
                message = Console.ReadLine() ?? "";
                client.SendMessage(message);
                WriteTimedMessage($"Me: {message}");
            }
            
            client.LeaveServer();
            Environment.Exit(0);
        }

        private static void UserKickHandler(object sender, KickedUserEventArgs e)
        {
            WriteTimedMessage($"User '{e.KickedUser.Name}' was kicked by '{e.KickedBy.Name}' for {e.Reason}.");
        }

        private static void NewUserListHandler(object sender, List<User> users)
        {
            WriteTimedMessage("The following users are already in the chat:");
            users.ForEach(user => WriteTimedMessage($" - {user.Name}"));
        }

        private static void UserLeaveHandler(object sender, User e)
        {
            WriteTimedMessage($"'{e.Name}' left the chat.");
        }

        private static void UserJoinHandler(object sender, User e)
        {
            WriteTimedMessage($"'{e.Name}' joined the chat.");
        }

        private static void ServerJoinHandler(object sender, EventArgs e)
        {
            WriteTimedMessage("Joined server.");
        }

        private static void ServerDisconnectHandler(object sender, EventArgs e)
        {
            WriteTimedMessage("Disconnected from server.");
        }

        private static void ServerConnectHandler(object sender, EventArgs e)
        {
            WriteTimedMessage("Server connection established.");
        }

        private static void WriteTimedMessage(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}");
        }

        private static void IncomingMessageHandler(object sender, IncomingMessageEventArgs e)
        {
            WriteTimedMessage($"{e.Sender.Name}: {e.Message}");
        }
    }
}