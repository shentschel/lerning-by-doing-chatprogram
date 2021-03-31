using System;
using CommunicationLibrary.Models;

namespace CommunicationLibrary.Events.Client
{
    public class IncomingMessageEventArgs : EventArgs
    {
        public User Sender { get; set; }
        
        public string Message { get; set; }
    }
}