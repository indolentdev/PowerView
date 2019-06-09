using System;

namespace PowerView.Model
{
  public interface IDisconnectRule : IEquatable<IDisconnectRule>
  {
    ISerieName Name { get; }
    ISerieName EvaluationName { get; }
    TimeSpan Duration { get; }
    int DisconnectToConnectValue { get; }
    int ConnectToDisconnectValue { get; }
    Unit Unit { get; }
  }
}
