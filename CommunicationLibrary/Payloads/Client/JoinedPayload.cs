using System;
using Newtonsoft.Json;

namespace CommunicationLibrary.Payloads.Client
{
    public class JoinedPayload : ClientPayload
    {
        [JsonProperty("userId")] public Guid UserId { get; }
        
        [JsonProperty("name")] public string Name { get; }
        
        [JsonProperty("color")] public string Color { get; }

        public JoinedPayload(Guid userId, string name, string color)
        {
            UserId = userId;
            Name = name;
            Color = color;
        }
    }
}