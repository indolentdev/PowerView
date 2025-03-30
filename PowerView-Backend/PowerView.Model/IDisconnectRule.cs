using System;

namespace PowerView.Model
{
    public interface IDisconnectRule : IEquatable<IDisconnectRule>
    {
        ISeriesName Name { get; }
        ISeriesName EvaluationName { get; }
        TimeSpan Duration { get; }
        int DisconnectToConnectValue { get; }
        int ConnectToDisconnectValue { get; }
        Unit Unit { get; }
    }
}
