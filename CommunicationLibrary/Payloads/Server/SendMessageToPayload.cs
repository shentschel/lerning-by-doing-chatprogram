using System;
using Newtonsoft.Json;

namespace CommunicationLibrary.Payloads.Server
{
    public class SendMessageToPayload : ServerPayload
    {
        [JsonProperty("message")] public string Message { get; }
        
        [JsonProperty("recipient")] public Guid Recipient { get; }

        public SendMessageToPayload(string message, Guid recipient)
        {
            Message = message;
            Recipient = recipient;
        }
    }
}