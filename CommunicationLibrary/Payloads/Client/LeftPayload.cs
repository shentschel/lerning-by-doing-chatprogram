using System;
using Newtonsoft.Json;

namespace CommunicationLibrary.Payloads.Client
{
    public class LeftPayload : ClientPayload
    {
        [JsonProperty("userId")] public Guid UserId { get; }
        
        [JsonProperty("reason")] public string Reason { get; }

        public LeftPayload(Guid userId, string reason)
        {
            UserId = userId;
            Reason = reason;
        }
    }
}