using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using log4net;

namespace PowerView.Model
{
  public class LabelSeries : IEnumerable<ObisCode>
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly Dictionary<ObisCode, IList<TimeRegisterValue>> obisCodeSets;

    public LabelSeries(string label, IDictionary<ObisCode, IEnumerable<TimeRegisterValue>> timeRegisterValuesByObisCode)
    {
      if (string.IsNullOrEmpty(label)) throw new ArgumentOutOfRangeException("label", "Must not be null or empty");
      if (timeRegisterValuesByObisCode == null) throw new ArgumentNullException("timeRegisterValuesByObisCode");
      Label = label;
      obisCodeSets = new Dictionary<ObisCode, IList<TimeRegisterValue>>(5);

      foreach (var obisCodeValueSet in timeRegisterValuesByObisCode)
      {
        if (obisCodeValueSet.Value == null) throw new ArgumentOutOfRangeException("label", obisCodeValueSet.Key + " has null value");

        var orderedTimeRegisterValues = obisCodeValueSet.Value.OrderBy(sv => sv.Timestamp).ToList();
        obisCodeSets.Add(obisCodeValueSet.Key, orderedTimeRegisterValues);
      }
    }

    public string Label { get; private set; }

    public ICollection<TimeRegisterValue> this[ObisCode obisCode]
    {
      get
      {
        return new ReadOnlyCollection<TimeRegisterValue>(obisCodeSets.ContainsKey(obisCode) ? obisCodeSets[obisCode] : new TimeRegisterValue[0]);
      }
    }

    public LabelSeries Normalize(Func<DateTime, DateTime> timeDivider)
    {
      if (timeDivider == null) throw new ArgumentNullException("timeDivider");

      var normalized = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>();
      foreach (var entry in obisCodeSets)
      {
        var obisCode = entry.Key;
        var values = entry.Value;
        var normalizedValues = values.Select(x => x.Normalize(timeDivider)).GroupBy(x => x.Timestamp).Select(x => x.First());
        normalized.Add(obisCode, normalizedValues);
      }
      return new LabelSeries(Label, normalized);
    }

    #region IEnumerable implementation

    public IEnumerator<ObisCode> GetEnumerator()
    {
      return obisCodeSets.Keys.GetEnumerator();
    }

    #endregion

    #region IEnumerable implementation

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


  }
}
