using System.Collections.Generic;
using CommunicationLibrary.Models;
using Newtonsoft.Json;

namespace CommunicationLibrary.Payloads.Client
{
    public class ListPayload : ClientPayload
    {
        [JsonProperty("users")] public List<User> Users { get; }

        public ListPayload(List<User> users)
        {
            Users = users;
        }
    }
}