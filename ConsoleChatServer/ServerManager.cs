using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CommunicationLibrary;
using CommunicationLibrary.Actions;
using CommunicationLibrary.Events.Client;
using CommunicationLibrary.Models;
using CommunicationLibrary.Packets;
using CommunicationLibrary.Payloads.Server;

namespace ConsoleChatServer
{
    public class ServerManager
    {
        private ChatClient Client { get; }
        private List<User> Users { get; } = new List<User>();
        
        public ServerManager(string ip, int port)
        {
            Client = new ChatClient(ip, port);
            Client.ServerConnect += OnServerConnect;
            Client.ServerDisconnect += OnServerDisconnect;
            Client.ServerJoin += OnServerJoin;
            Client.UserJoin += OnUserJoin;
            Client.UserKick += OnUserKick;
            Client.UserLeave += OnUserLeave;
            Client.IncomingMessage += OnIncomingMessage;
            
            Client.JoinServer("Administrator", "#FF000000");
        }

        #region Event Handling
        private void OnServerConnect(object sender, EventArgs e)
        {
            WriteTimedMessage("Server connection established");
        }

        private void OnServerDisconnect(object sender, EventArgs e)
        {
            WriteTimedMessage("Disconnected from Server");
        }

        private void OnServerJoin(object sender, EventArgs e)
        {
            WriteTimedMessage("Server join was successful");
        }

        private void OnUserJoin(object sender, User e)
        {
            WriteTimedMessage($"'{e.Name}' joined the server.");
            Users.Add(e);
        }

        private void OnUserKick(object sender, KickedUserEventArgs e)
        {
            var kickedUser = Users.Find(user => user.Id == e.KickedUser.Id);
            if(kickedUser == null) return;

            var kickedBy = Users.Find(user => user.Id == e.KickedBy.Id);
            if (kickedBy == null) return;

            Users.Remove(kickedUser);
            
            WriteTimedMessage($"'{kickedUser.Name}' was kicked from server by '{kickedBy.Name}. Reason: {e.Reason}");
        }

        private void OnUserLeave(object sender, User e)
        {
            WriteTimedMessage($"'{e.Name}' left the server");
            var foundUser = Users.Find(user => user.Id == e.Id);
            if (foundUser == null) return;
            
            Users.Remove(foundUser);
        }

        private void OnIncomingMessage(object sender, IncomingMessageEventArgs e)
        {
            WriteTimedMessage($"'{e.Sender.Name} dared to write to the almighty Admin: {e.Message}");
        }
        
        private static void WriteTimedMessage(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}");
        }
        
        #endregion

        public void KickUser(Guid userId, string reason)
        {
            Client.KickUser(userId, reason);
        }

        public void HandleCommand(string command)
        {
            if(string.IsNullOrEmpty(command)) return;

            var commandParts = command.Split(' ');
            if (commandParts.Length < 2) return;

            var foundUser = Users.Find(user => user.Name.ToLower().Equals(commandParts[1].ToLower()));
            if (foundUser == null) return;

            var reason = "";
            if (commandParts.Length > 2)
            {
                var reasonParts = commandParts.Skip(2).ToArray();
                reason = string.Join(' ', reasonParts);
            }
            
            KickUser(foundUser.Id, reason);
        }
    }
}