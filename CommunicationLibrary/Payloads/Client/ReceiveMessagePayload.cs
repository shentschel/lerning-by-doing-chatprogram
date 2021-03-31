using System;
using Newtonsoft.Json;

namespace CommunicationLibrary.Payloads.Client
{
    public class ReceiveMessagePayload : ClientPayload
    {
        [JsonProperty("userId")] public Guid UserId { get; }
        
        [JsonProperty("message")] public string Message { get; }

        public ReceiveMessagePayload(Guid userId, string message)
        {
            UserId = userId;
            Message = message;
        }
    }
}