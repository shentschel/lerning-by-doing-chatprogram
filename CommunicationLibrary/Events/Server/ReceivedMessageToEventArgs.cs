using System;
using CommunicationLibrary.Models;

namespace CommunicationLibrary.Events.Server
{
    public class ReceivedMessageToEventArgs : ReceivedMessageEventArgs
    {
        public User Recipient { get; set; }
    }
}