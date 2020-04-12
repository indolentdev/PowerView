using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using log4net;

namespace PowerView.Model
{
  public class LabelSeries<T> : IEnumerable<ObisCode> where T: struct, ISeries
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly Dictionary<ObisCode, IList<T>> obisCodeSets;

    public LabelSeries(string label, IDictionary<ObisCode, IEnumerable<T>> timeRegisterValuesByObisCode)
    {
      if (string.IsNullOrEmpty(label)) throw new ArgumentOutOfRangeException("label", "Must not be null or empty");
      if (timeRegisterValuesByObisCode == null) throw new ArgumentNullException("timeRegisterValuesByObisCode");
      Label = label;
      obisCodeSets = new Dictionary<ObisCode, IList<T>>(5);

      foreach (var obisCodeValueSet in timeRegisterValuesByObisCode)
      {
        if (obisCodeValueSet.Value == null) throw new ArgumentOutOfRangeException("label", obisCodeValueSet.Key + " has null value");

        obisCodeSets.Add(obisCodeValueSet.Key, GetOrderedReadOnlyList(obisCodeValueSet.Value));
      }
    }

    private static IList<T> GetOrderedReadOnlyList(IEnumerable<T> values)
    {
      return new ReadOnlyCollection<T>(values.OrderBy(sv => sv.OrderProperty).ToList());
    }

    public string Label { get; private set; }

    public bool ContainsObisCode(ObisCode obisCode)
    {
      return obisCodeSets.ContainsKey(obisCode);
    }

    public ICollection<T> this[ObisCode obisCode]
    {
      get
      {
        return obisCodeSets.ContainsKey(obisCode) ? obisCodeSets[obisCode] : new T[0];
      }
    }

    public IDictionary<ObisCode, IList<T>> GetCumulativeSeries()
    {
      return obisCodeSets.Where(x => x.Key.IsCumulative).ToDictionary(x => x.Key, x => x.Value);
    }

    public LabelSeries<NormalizedTimeRegisterValue> Normalize(Func<DateTime, DateTime> timeDivider)
    {
      if (timeDivider == null) throw new ArgumentNullException("timeDivider");

      var normalized = new Dictionary<ObisCode, IEnumerable<NormalizedTimeRegisterValue>>();
      foreach (var entry in obisCodeSets)
      {
        var obisCode = entry.Key;
        var values = entry.Value;
        // The GroupBy and Select(x.First()) relies on the ordering provided by GetOrderedReadOnlyList above.
        // Confer the MSDN remark for GroupBy: 
        // https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable.groupby?redirectedfrom=MSDN&view=netframework-4.8#System_Linq_Enumerable_GroupBy__3_System_Collections_Generic_IEnumerable___0__System_Func___0___1__System_Func___0___2__
        var normalizedValues = values.Select(x => x.Normalize(timeDivider)).GroupBy(x => x.NormalizedTimestamp).Select(x => x.First());
        normalized.Add(obisCode, normalizedValues);
      }
      return new LabelSeries<NormalizedTimeRegisterValue>(Label, normalized);
    }

    public void Add(IDictionary<ObisCode, IList<T>> series)
    {
      foreach (var s in series)
      {
        obisCodeSets.Add(s.Key, GetOrderedReadOnlyList(s.Value));
      }
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
