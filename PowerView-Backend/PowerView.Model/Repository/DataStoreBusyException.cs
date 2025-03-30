using System;
using System.Runtime.Serialization;

namespace PowerView.Model.Repository
{
    [Serializable]
    public class DataStoreBusyException : DataStoreException
    {
        public DataStoreBusyException()
        {
        }

        public DataStoreBusyException(string message)
          : base(message)
        {
        }

        public DataStoreBusyException(string message, Exception inner)
          : base(message, inner)
        {
        }
    }
}

