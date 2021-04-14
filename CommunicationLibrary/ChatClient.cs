using System;
using System.Collections.Generic;
using System.Text;
using CommunicationLibrary.Actions;
using CommunicationLibrary.Events.Client;
using CommunicationLibrary.Exceptions.Client;
using CommunicationLibrary.Models;
using CommunicationLibrary.Packets;
using CommunicationLibrary.Payloads.Client;
using CommunicationLibrary.Payloads.Server;
using log4net;
using WatsonTcp;

namespace CommunicationLibrary
{
    public sealed class ChatClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChatClient));

        public event EventHandler ServerConnect;
        public event EventHandler ServerDisconnect;
        public event EventHandler ServerJoin;
        public event EventHandler<User> UserJoin;
        public event EventHandler<User> UserLeave;
        public event EventHandler<KickedUserEventArgs> UserKick;
        public event EventHandler<IncomingMessageEventArgs> IncomingMessage;
        public event EventHandler<List<User>> NewUserList;

        private string Ip { get; }
        private int Port { get; }

        private List<User> Users { get; set; } = new List<User>();

        private readonly WatsonTcpClient _client;

        public ChatClient(string ip = "127.0.0.1", int port = 13000) 
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

            _client = new WatsonTcpClient(Ip, Port);
            _client.Events.ServerConnected += ServerConnected;
            _client.Events.ServerDisconnected += ServerDisconnected;
            _client.Events.MessageReceived += MessageReceived;
        }

        #region Event Invokation

        private void InvokeServerConnectEvent()
        {
            ServerConnect?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeServerDisconnectEvent()
        {
            ServerDisconnect?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeServerJoinEvent()
        {
            ServerJoin?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeUserJoinEvent(User user)
        {
            UserJoin?.Invoke(this, user);
        }

        private void InvokeUserLeaveEvent(User user)
        {
            UserLeave?.Invoke(this, user);
        }

        private void InvokeUserKickEvent(User kickedBy, User kickedUser, string reason)
        {
            var eventArgs = new KickedUserEventArgs
            {
                KickedBy = kickedBy,
                KickedUser = kickedUser,
                Reason = reason
            };
            
            UserKick?.Invoke(this, eventArgs);
        }

        private void InvokeIncomingMessageEvent(User sender, string message)
        {
            var eventArgs = new IncomingMessageEventArgs
            {
                Sender = sender,
                Message = message
            };

            IncomingMessage?.Invoke(this, eventArgs);
        }

        private void InvokeNewUserListEvent(List<User> users)
        {
            NewUserList?.Invoke(this, users);
        }

        #endregion

        #region Event Handling
        
        private void ServerConnected(object sender, ConnectionEventArgs e)
        {
            Logger.Info($"Client connected to server '{Ip}:{Port}'.");
            InvokeServerConnectEvent();
        }

        private void ServerDisconnected(object sender, DisconnectionEventArgs e)
        {
            Logger.Info($"Client disconnected from server '{Ip}:{Port}'. Reason: '{e.Reason.ToString()}'");
            InvokeServerDisconnectEvent();
        }

        private void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var dataString = Encoding.UTF8.GetString(e.Data);
            var clientPacket = ClientPacket.ToPacket(dataString);

            ProcessMessage(clientPacket);
        }

        private void ProcessMessage(ClientPacket clientPacket)
        {
            var action = clientPacket.Action;
            var payload = clientPacket.Payload;

            switch (action)
            {
                case ClientAction.Joined:
                {
                    AddUser(payload as JoinedPayload);
                    break;
                }
                case ClientAction.Kicked:
                {
                    KickUser(payload as KickedPayload);
                    break;
                }
                case ClientAction.Left:
                {
                    RemoveUser(payload as LeftPayload);
                    break;
                }
                case ClientAction.UserList:
                {
                    UpdateUserList(payload as ListPayload);
                    break;
                }
                case ClientAction.ReceiveMessage:
                {
                    ReceiveMessage(payload as ReceiveMessagePayload);
                    break;
                }
                default:
                {
                    Logger.Warn($"Unable to parse message from server '{Ip}:{Port}'.");
                    break;
                }
            }
        }

        private void AddUser(JoinedPayload payload)
        {
            var newUser = new User
            {
                Id = payload.UserId,
                Color = payload.Color,
                Name = payload.Name
            };
            
            Users.Add(newUser);
            InvokeUserJoinEvent(newUser);
        }

        private void KickUser(KickedPayload payload)
        {
            var kickedUser = Users.Find(user => user.Id == payload.UserId);
            if(kickedUser == null) return;

            var kickedBy = Users.Find(user => user.Id == payload.KickedBy);
            if(kickedBy == null) return;

            Users.Remove(kickedUser);
            InvokeUserKickEvent(kickedBy, kickedUser, payload.Reason);
        }

        private void RemoveUser(LeftPayload payload)
        {
            var removeUser = Users.Find(user => user.Id == payload.UserId);
            if (removeUser == null) return;

            Users.Remove(removeUser);
            InvokeUserLeaveEvent(removeUser);
        }

        private void UpdateUserList(ListPayload payload)
        {
            Users = payload.Users;
            InvokeNewUserListEvent(Users);
        }

        private void ReceiveMessage(ReceiveMessagePayload payload)
        {
            var sender = Users.Find(user => user.Id == payload.UserId);
            InvokeIncomingMessageEvent(sender, payload.Message);
        }

        #endregion

        public void JoinServer(string name, string color)
        {
            var checkUser = Users.Find(existingUser => existingUser.Name.Equals(name));
            if (checkUser != null)
            {
                throw new UserAlreadyExistsException(name);
            }
            
            var user = new User
            {
                Name = name,
                Color = color
            };

            var payload = new JoinPayload(user.Name, user.Color);
            var serverPacket = new ServerPacket(ServerAction.Join, payload);
            
            _client.Connect();
            _client.SendAsync(serverPacket.ToJson());
            InvokeServerJoinEvent();
        }

        public void LeaveServer()
        {
            Users.Clear();
            _client.Disconnect();
        }

        public void SendMessage(string message)
        {
            var payload = new SendMessagePayload(message);
            var serverPacket = new ServerPacket(ServerAction.SendMessage, payload);
            _client.SendAsync(serverPacket.ToJson());
        }

        public void SendMessageTo(string message, Guid recipient)
        {
            var payload = new SendMessageToPayload(message, recipient);
            var serverPacket = new ServerPacket(ServerAction.SendMessage, payload);
            _client.SendAsync(serverPacket.ToJson());
        }
    }
}