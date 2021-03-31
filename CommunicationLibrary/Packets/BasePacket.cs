using System;
using Newtonsoft.Json;

namespace CommunicationLibrary.Packets
{
    public class BasePacket<T> : IPacket
    {
        public string ToJson()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            };

            return JsonConvert.SerializeObject(this, settings);
        }

        public static T ToPacket(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("Parameter cannot be null, empty or only whitespaces.", nameof(json));
            }
            
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
    }
}