using System;
using System.Collections.Generic;
using System.Text;
using CommunicationLibrary.Actions;
using CommunicationLibrary.Models;
using CommunicationLibrary.Packets;
using CommunicationLibrary.Payloads.Client;
using CommunicationLibrary.Payloads.Server;
using log4net;
using WatsonTcp;

namespace CommunicationLibrary
{
    public class ChatServer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChatServer));

        public event EventHandler<User> UserJoin;
        public event EventHandler<User> UserLeave;
        public event EventHandler ServerStop;
        public event EventHandler ServerStart;
        
        private string Ip { get; }
        private int Port { get; }

        private List<User> Users { get; } = new List<User>();
        
        private readonly WatsonTcpServer _server;

        public ChatServer(string ip = "127.0.0.1", int port = 13000)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                throw new ArgumentException("Parameter cannot be null, empty or only whitespaces.", nameof(ip));
            }
            
            if (port <= 1024 || port >= 65535)
            {
                throw new ArgumentException(
                    "Parameter needs to be greater than 1024 and smaller than 65535.",
                    nameof(port));
            } 
            
            Ip = ip;
            Port = port;
            
            _server = new WatsonTcpServer(Ip, Port);
            _server.Events.ClientConnected += OnClientConnected;
            _server.Events.ClientDisconnected += OnClientDisconnected;
            _server.Events.ServerStarted += OnServerStarted;
            _server.Events.ServerStopped += OnServerStopped;
            _server.Events.MessageReceived += OnMessageReceived;
        }

        #region Event Invokation
        protected virtual void InvokeUserJoinEvent(User user)
        {
            UserJoin?.Invoke(this, user);
        }

        protected virtual void InvokeUserLeaveEvent(User user)
        {
            UserLeave?.Invoke(this, user);
        }

        protected virtual void InvokeServerStopEvent()
        {
            ServerStop?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void InvokeServerStartEvent()
        {
            ServerStart?.Invoke(this, EventArgs.Empty);
        }
        #endregion
        
        #region Event Handling
        private void OnClientConnected(object sender, ConnectionEventArgs e)
        {
            Logger.Info($"Incoming client connection from '{e.IpPort}'.");

            SendUserList(e.IpPort);
        }

        private void SendUserList(string ipPort)
        {
            var userListPacket = new ClientPacket(ClientAction.UserList, new ListPayload(Users));
            _server.SendAsync(ipPort, userListPacket.ToJson());
        }

        private void OnClientDisconnected(object sender, DisconnectionEventArgs e)
        {
            Logger.Info($"client '{e.IpPort}' disconnected. Reason: {e.Reason.ToString()} ");

            var removedUser = RemoveClientFromUserList(e.IpPort);
            InformOtherClients(removedUser, e.Reason.ToString());
        }

        private User RemoveClientFromUserList(string ipPort)
        {
            var removeUser = Users.Find(user => user.IpPort.Equals(ipPort));
            
            if (removeUser != null)
            {
                Users.Remove(removeUser);
            }

            return removeUser;
        }

        private void InformOtherClients(User user, string reason)
        {
            if(user == null) return;

            var payload = new LeftPayload(user.Id, reason);
            var packet = new ClientPacket(ClientAction.Left, payload);
            
            SendToAllUsers(packet);
        }

        private void SendToAllUsers(ClientPacket packet)
        {
            Users.ForEach(user => _server.SendAsync(user.IpPort, packet.ToJson()));
        }

        private void OnServerStarted(object sender, EventArgs e)
        {
            Logger.Info($"Server started. Listening on '{Ip}:{Port}'.");
            Users.Clear();
            InvokeServerStartEvent();
        }

        private void OnServerStopped(object sender, EventArgs e)
        {
            Logger.Info("Server stopped.");
            Users.Clear();
            InvokeServerStopEvent();
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var dataString = Encoding.UTF8.GetString(e.Data);
            var serverPacket = ServerPacket.ToPacket(dataString);

            ProcessMessage(e.IpPort, serverPacket);
        }

        private void ProcessMessage(string ipPort, ServerPacket packet)
        {
            var action = packet.Action;
            var payload = packet.Payload;
            
            switch (action)
            {
                case ServerAction.Join:
                {
                    AddUser(ipPort, payload as JoinPayload);
                    break;
                }
                case ServerAction.Kick:
                {
                    KickUser(ipPort, payload as KickPayload);
                    break;
                }
                case ServerAction.SendMessage:
                {
                    SendMessage(ipPort, payload as SendMessagePayload);
                    break;
                }
                case ServerAction.SendMessageTo:
                {
                    SendMessageTo(ipPort, payload as SendMessageToPayload);
                    break;
                }
                default:
                {
                    Logger.Warn($"Unable to perform Action '{action}' from '{ipPort}'.");
                    break;
                }
            }
        }

        private void AddUser(string ipPort, JoinPayload payload)
        {
            Logger.Info($"Client from '{ipPort}' wants to join chat server.");
            if (string.IsNullOrWhiteSpace(payload.Name) || string.IsNullOrWhiteSpace(payload.Color))
            {
                Logger.Debug($"Client '{ipPort}' has no name or no color.");
                _server.DisconnectClient(ipPort, MessageStatus.Failure);
                return;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = payload.Name,
                Color = payload.Color,
                IpPort = ipPort
            };
            var clientPayload = new JoinedPayload(user.Id, user.Name, user.Color);
            var clientPacket = new ClientPacket(ClientAction.Joined, clientPayload);
            
            SendToAllUsers(clientPacket);
            
            Users.Add(user);
            InvokeUserJoinEvent(user);
        }

        private void KickUser(string ipPort, KickPayload payload)
        {
            var kickedBy = Users.Find(user => user.IpPort.Equals(ipPort));
            if(kickedBy == null) return;

            var foundUser = Users.Find(user => user.Id == payload.UserId);
            if(foundUser == null) return;

            Logger.Info($"User '{foundUser.Name} - {foundUser.Id}' was kicked by '{kickedBy.Name} - {kickedBy.Id}'. Reason: '{payload.Reason}'");
            
            Users.Remove(foundUser);
            _server.DisconnectClient(foundUser.IpPort);

            var kickedPayload = new KickedPayload(foundUser.Id, payload.Reason, kickedBy.Id);
            var clientPacket = new ClientPacket(ClientAction.Kicked, kickedPayload);
            
            SendToAllUsers(clientPacket);
            InvokeUserLeaveEvent(foundUser);
        }

        private void SendMessage(string ipPort, SendMessagePayload payload)
        {
            var sender = Users.Find(user => user.IpPort.Equals(ipPort));
            if(sender == null) return;

            var messagePayload = new ReceiveMessagePayload(sender.Id, payload.Message);
            var clientPacket = new ClientPacket(ClientAction.ReceiveMessage, messagePayload);
            
            SendToAllUsers(clientPacket);
        }

        private void SendMessageTo(string ipPort, SendMessageToPayload payload)
        {
            var sender = Users.Find(user => user.IpPort.Equals(ipPort));
            if(sender == null) return;

            var recipient = Users.Find(user => user.Id == payload.Recipient);
            if(recipient == null) return;
            
            var messagePayload = new ReceiveMessagePayload(sender.Id, payload.Message);
            var clientPacket = new ClientPacket(ClientAction.ReceiveMessage, messagePayload);

            _server.SendAsync(recipient.IpPort, clientPacket.ToJson());
        }

        #endregion

        public void StartServer()
        {
            _server.Start();
        }

        public void StopServer()
        {
            _server.Stop();
        }
    }
}