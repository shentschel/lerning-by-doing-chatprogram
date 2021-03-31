using CommunicationLibrary.Actions;
using CommunicationLibrary.Payloads.Client;
using Newtonsoft.Json;

namespace CommunicationLibrary.Packets
{
    public class ClientPacket : BasePacket<ClientPacket>
    {
        [JsonProperty("action")] public ClientAction Action { get; }
        
        [JsonProperty("payload")] public ClientPayload Payload { get; }

        public ClientPacket(ClientAction action, ClientPayload payload)
        {
            Action = action;
            Payload = payload;
        }
    }
}