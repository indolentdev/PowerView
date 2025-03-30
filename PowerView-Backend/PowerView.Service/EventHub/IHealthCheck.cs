using System;
using Microsoft.Extensions.DependencyInjection;

namespace PowerView.Service.EventHub
{
    public interface IHealthCheck
    {
        void DailyCheck(IServiceScope serviceScope, DateTime dateTime);
    }
}

