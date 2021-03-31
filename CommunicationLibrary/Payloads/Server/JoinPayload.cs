using Newtonsoft.Json;

namespace CommunicationLibrary.Payloads.Server
{
    public class JoinPayload : ServerPayload
    {
        [JsonProperty("name")] public string Name { get; }
        
        [JsonProperty("color")] public string Color { get; }

        public JoinPayload(string name, string color)
        {
            Name = name;
            Color = color;
        }
    }
}