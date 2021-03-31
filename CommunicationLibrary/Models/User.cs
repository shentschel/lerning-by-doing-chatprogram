using System;
using Newtonsoft.Json;

namespace CommunicationLibrary.Models
{
    public class User
    {
        [JsonProperty("name")] public string Name { get; set; }
        
        [JsonProperty("color")] public string Color { get; set; }
        
        [JsonProperty("id")] public Guid Id { get; set; }
        
        [JsonIgnore] public string IpPort { get; set; }
    }
}