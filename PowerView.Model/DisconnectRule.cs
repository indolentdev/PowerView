using System;

namespace PowerView.Model
{
  public class DisconnectRule : IDisconnectRule, IEquatable<DisconnectRule>
  {
    private readonly ISerieName name;
    private readonly ISerieName evaluationName;
    private readonly TimeSpan duration;
    private readonly int disconnectToConnectValue;
    private readonly int connectToDisconnectValue;
    private readonly Unit unit;

    public DisconnectRule(ISerieName name, ISerieName evaluationName, TimeSpan duration, int disconnectToConnectValue, int connectToDisconnectValue, Unit unit)
    {
      if (name == null) throw new ArgumentNullException("name");
      if (evaluationName == null) throw new ArgumentNullException("evaluationName");
      if (duration.TotalMinutes < 15 || duration.TotalMinutes > 6 * 60) throw new ArgumentOutOfRangeException("duration", duration, "Must be between 2 mins and 6 hours");
      if (disconnectToConnectValue < 1) throw new ArgumentOutOfRangeException("disconnectToConnectValue", disconnectToConnectValue, "Must be greater than zero");
      if (connectToDisconnectValue < 1) throw new ArgumentOutOfRangeException("connectToDisconnectValue", connectToDisconnectValue, "Must be greater than zero");
      if (disconnectToConnectValue <= connectToDisconnectValue) throw new ArgumentOutOfRangeException("connectToDisconnectValue", connectToDisconnectValue, "Must be greater than disconnectToConnectValue");
      if (!Enum.IsDefined(typeof(Unit), unit)) throw new ArgumentOutOfRangeException("unit", unit, "Must be a valid unit");

      this.name = name;
      this.evaluationName = evaluationName;
      this.duration = duration;
      this.disconnectToConnectValue = disconnectToConnectValue;
      this.connectToDisconnectValue = connectToDisconnectValue;
      this.unit = unit;
    }

    public ISerieName Name { get { return name; } }
    public ISerieName EvaluationName { get { return evaluationName; } }
    public TimeSpan Duration { get { return duration; } }
    public int DisconnectToConnectValue { get { return disconnectToConnectValue; } }
    public int ConnectToDisconnectValue { get { return connectToDisconnectValue; } }
    public Unit Unit { get { return unit; } }

    public override bool Equals(object obj)
    {
      if (!(obj is DisconnectRule)) return false;
      return Equals((DisconnectRule)obj);
    }

    public bool Equals(DisconnectRule other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Name.Equals(other.Name) && EvaluationName.Equals(other.EvaluationName) &&
                 Duration.Equals(other.Duration) &&
                 DisconnectToConnectValue.Equals(other.DisconnectToConnectValue) && 
                 ConnectToDisconnectValue.Equals(other.ConnectToDisconnectValue) &&
                 Unit.Equals(other.Unit);
    }

    public bool Equals(IDisconnectRule other)
    {
      return Equals((object)other);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = Name.GetHashCode();
        hashCode = (hashCode * 397) ^ EvaluationName.GetHashCode();
        hashCode = (hashCode * 397) ^ Duration.GetHashCode();
        hashCode = (hashCode * 397) ^ DisconnectToConnectValue.GetHashCode();
        hashCode = (hashCode * 397) ^ ConnectToDisconnectValue.GetHashCode();
        hashCode = (hashCode * 397) ^ Unit.GetHashCode();
        return hashCode;
      }
    }

  }
}
