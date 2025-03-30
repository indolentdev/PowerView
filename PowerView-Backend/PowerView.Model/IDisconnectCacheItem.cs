using System;
using System.Collections.Generic;

namespace PowerView.Model
{
    public interface IDisconnectCacheItem
    {
        IDisconnectRule Rule { get; }
        bool Connected { get; }
        int Count { get; }

        void Add(IEnumerable<TimeRegisterValue> values);
        void Calculate(DateTime time);
    }
}
