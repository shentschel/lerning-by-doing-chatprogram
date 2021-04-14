using System;
using CommunicationLibrary.Models;

namespace CommunicationLibrary.Events.Server
{
    public class ReceivedMessageEventArgs : EventArgs
    {
        public User Sender { get; set; }
        
        public string Message { get; set; }
    }
}