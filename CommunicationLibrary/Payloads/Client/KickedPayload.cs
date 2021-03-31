using System;
using Newtonsoft.Json;

namespace CommunicationLibrary.Payloads.Client
{
    public class KickedPayload : ClientPayload
    {
        [JsonProperty("userId")] public Guid UserId { get; }
        
        [JsonProperty("reason")] public string Reason { get; }
        
        [JsonProperty("kickedBy")] public Guid KickedBy { get; }

        public KickedPayload(Guid userId, string reason, Guid kickedBy)
        {
            UserId = userId;
            Reason = reason;
            KickedBy = kickedBy;
        }
    }
}