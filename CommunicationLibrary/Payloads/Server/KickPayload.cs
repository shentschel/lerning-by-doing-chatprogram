using System;
using Newtonsoft.Json;

namespace CommunicationLibrary.Payloads.Server
{
    public class KickPayload : ServerPayload
    {
        [JsonProperty("userId")] public Guid UserId { get; }
        
        [JsonProperty("reason")] public string Reason { get; }

        public KickPayload(Guid userId, string reason)
        {
            UserId = userId;
            Reason = reason;
        }
    }
}