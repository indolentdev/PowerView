using System;

namespace PowerView.Service
{
    public interface IServiceHost : IDisposable
    {
        void Start();
    }
}

