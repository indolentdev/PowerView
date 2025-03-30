using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PowerView.Model
{
    public class LabelSeries<T> : IEnumerable<ObisCode> where T : class, IRegisterValue
    {
        private readonly Dictionary<ObisCode, IList<T>> obisCodeSets;

        public LabelSeries(string label, IDictionary<ObisCode, IEnumerable<T>> timeRegisterValuesByObisCode)
        {
            ArgCheck.ThrowIfNullOrEmpty(label);
            ArgumentNullException.ThrowIfNull(timeRegisterValuesByObisCode);

            Label = label;
            obisCodeSets = new Dictionary<ObisCode, IList<T>>(5);

            foreach (var obisCodeValueSet in timeRegisterValuesByObisCode)
            {
                if (obisCodeValueSet.Value == null) throw new ArgumentOutOfRangeException(nameof(label), obisCodeValueSet.Key + " has null value");

                obisCodeSets.Add(obisCodeValueSet.Key, GetOrderedReadOnlyList(obisCodeValueSet.Value));
            }
        }

        private static ReadOnlyCollection<T> GetOrderedReadOnlyList(IEnumerable<T> values)
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
                return obisCodeSets.TryGetValue(obisCode, out var val) ? val : Array.Empty<T>();
            }
        }

        public IDictionary<ObisCode, IList<T>> GetCumulativeSeries()
        {
            return obisCodeSets.Where(x => x.Key.IsCumulative).ToDictionary(x => x.Key, x => x.Value);
        }

        public IDictionary<ObisCode, IList<T>> GetNonCumulativeSeries()
        {
            return obisCodeSets.Except(GetCumulativeSeries()).ToDictionary(x => x.Key, x => x.Value);
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
