using CommunicationLibrary.Actions;
using CommunicationLibrary.Payloads.Server;
using Newtonsoft.Json;

namespace CommunicationLibrary.Packets
{
    public class ServerPacket : BasePacket<ServerPacket>
    {
        [JsonProperty("action")] public ServerAction Action { get; }
        
        [JsonProperty("payload")] public ServerPayload Payload { get; }

        public ServerPacket(ServerAction action, ServerPayload payload)
        {
            Action = action;
            Payload = payload;
        }
    }
}