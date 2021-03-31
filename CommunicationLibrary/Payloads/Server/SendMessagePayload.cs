using System;
using Newtonsoft.Json;

namespace CommunicationLibrary.Payloads.Server
{
    public class SendMessagePayload : ServerPayload
    {
        [JsonProperty("message")] public string Message { get; }

        public SendMessagePayload(string message)
        {
            Message = message;
        }
    }
}