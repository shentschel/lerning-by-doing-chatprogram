using System;

namespace CommunicationLibrary.Exceptions.Client
{
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string name) : base($"User with name {name} already exists.")
        { }
    }
}