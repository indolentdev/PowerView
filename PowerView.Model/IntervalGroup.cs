using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PowerView.Model.Expression;

namespace PowerView.Model
{
  public class IntervalGroup
  {
    private readonly Func<DateTime, DateTime> timeDivider;
    private readonly Func<DateTime, DateTime> getNext;

    public IntervalGroup(string interval, IList<ProfileGraph> profileGraphs, LabelSeriesSet<TimeRegisterValue> labelSeriesSet)
    {
      timeDivider = DateTimeResolutionDivider.GetResolutionDivider(interval);
      getNext = DateTimeResolutionDivider.GetNext(interval);
      if (profileGraphs == null) throw new ArgumentNullException("profileGraphs");
      if (labelSeriesSet == null) throw new ArgumentNullException("labelSeriesSet");

      Interval = interval;
      ProfileGraphs = new ReadOnlyCollection<ProfileGraph>(profileGraphs);
      LabelSeriesSet = labelSeriesSet;
    }

    public string Interval { get; private set; }
    public ICollection<ProfileGraph> ProfileGraphs { get; private set; }
    public LabelSeriesSet<TimeRegisterValue> LabelSeriesSet { get; private set; }

    public IList<DateTime> Categories { get; private set; }
    public IList<DateTime> NormalizedCategories { get; private set; }
    public LabelSeriesSet<NormalizedTimeRegisterValue> NormalizedLabelSeriesSet { get; private set; }

    public void Prepare(ICollection<LabelObisCodeTemplate> labelObisCodeTemplates)
    {
      var categories = GetCategories();
      Categories = new ReadOnlyCollection<DateTime>(categories);
      NormalizedCategories = new ReadOnlyCollection<DateTime>(categories.Select(timeDivider).ToList());

      GenerateSeriesFromCumulative();

      NormalizedLabelSeriesSet = LabelSeriesSet.Normalize(timeDivider);
      GenerateLabelsFromTemplates(labelObisCodeTemplates);
    }

    private List<DateTime> GetCategories()
    {
      var categories = new List<DateTime>();
      var categoryTimestamp = LabelSeriesSet.Start;
      while (categoryTimestamp < LabelSeriesSet.End)
      {
        categories.Add(categoryTimestamp);
        categoryTimestamp = getNext(categoryTimestamp);
      }

      return categories;
    }

    private void GenerateSeriesFromCumulative()
    {
      foreach (var labelSeries in LabelSeriesSet)
      {
        var generator = new SeriesFromCumulativeGenerator();
        labelSeries.Add(generator.Generate(labelSeries.GetCumulativeSeries()));
      }
    }

    private void GenerateLabelsFromTemplates(ICollection<LabelObisCodeTemplate> labelObisCodeTemplates)
    {
      var generator = new LabelSeriesFromTemplatesGenerator(labelObisCodeTemplates);
      NormalizedLabelSeriesSet.Add(generator.Generate(NormalizedLabelSeriesSet));
    }
  }
}
