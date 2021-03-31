using System;
using CommunicationLibrary;

namespace ConsoleChatClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter Server IP and Port (Format: 'IP:Port'):");
            var ipPort = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ipPort))
            {
                Console.WriteLine("No IP:Port defined.");
                Environment.Exit(-1);
            }

            var connectionDetails = ipPort.Split(';');
            var client = new ChatClient(connectionDetails[0], int.Parse(connectionDetails[1]));

            client.IncomingMessage += IncomingMessageHandler;
            client.ServerConnect += ServerConnectHandler;
            client.ServerDisconnect += ServerDisconnectHandler;
            client.ServerJoin += ServerJoinHandler;
            client.UserJoin += UserJoinHandler;
            client.UserLeave += UserLeaveHandler;
            client.NewUserList += NewUserListHandler;
            client.UserKick += UserKickHandler;
            
            client.

        }
    }
}