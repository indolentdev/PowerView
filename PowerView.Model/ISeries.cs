using System;
namespace PowerView.Model
{
  public interface ISeries
  {
    DateTime OrderProperty { get; }

    NormalizedTimeRegisterValue Normalize(Func<DateTime, DateTime> timeDivider);
  }
}
