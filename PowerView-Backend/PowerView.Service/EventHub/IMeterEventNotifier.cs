using System;

namespace PowerView.Service.EventHub
{
    public interface IMeterEventNotifier
    {
        void NotifyEmailRecipients();
    }
}
