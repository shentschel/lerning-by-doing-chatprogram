using System;
using CommunicationLibrary.Models;

namespace CommunicationLibrary.Events.Client
{
    public class KickedUserEventArgs : EventArgs
    {
        public User KickedBy { get; set; }
        
        public User KickedUser { get; set; }
        
        public string Reason { get; set; }
    }
}