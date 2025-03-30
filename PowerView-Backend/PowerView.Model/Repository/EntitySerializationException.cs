using System;
using System.Runtime.Serialization;

namespace PowerView.Model.Repository
{
    [Serializable]
    public class EntitySerializationException : Exception
    {
        public EntitySerializationException() : base()
        {
        }

        public EntitySerializationException(string message) : base(message)
        {
        }

        public EntitySerializationException(string message, Exception e) : base(message, e)
        {
        }
    }
}

