using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace PowerView.Model
{
  public interface ILeakCharacteristicChecker
  {
    UnitValue? GetLeakCharacteristic(LabelSeries<NormalizedDurationRegisterValue> labelSeries, ObisCode obisCode, DateTime start, DateTime end);
  }
}
