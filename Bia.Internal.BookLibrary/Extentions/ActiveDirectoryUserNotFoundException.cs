using System;
using System.Runtime.Serialization;

namespace Bia.Internal.BookLibrary
{
    [Serializable]
    public class ActiveDirectoryUserNotFoundException : Exception
    {
        public ActiveDirectoryUserNotFoundException() : base()
        {
        }

        public ActiveDirectoryUserNotFoundException(string message) : base(message)
        {
        }

        public ActiveDirectoryUserNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ActiveDirectoryUserNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message => base.Message.Equals(string.Empty) ? "User does not exists in ActiveDirectory" : base.Message;
    }
}
