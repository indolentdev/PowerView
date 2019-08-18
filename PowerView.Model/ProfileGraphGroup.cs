using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PowerView.Model.Expression;

namespace PowerView.Model
{
  public class ProfileGraphGroup
  {
    private readonly Func<DateTime, DateTime> timeDivider;
    private readonly Func<DateTime, DateTime> getNext;

    public ProfileGraphGroup(string interval, IList<ProfileGraph> profileGraphs, LabelSeriesSet sourceLabelSeriesSet)
    {
      timeDivider = DateTimeResolutionDivider.GetResolutionDivider(interval);
      getNext = DateTimeResolutionDivider.GetNext(interval);
      if (profileGraphs == null) throw new ArgumentNullException("profileGraphs");
      if (sourceLabelSeriesSet == null) throw new ArgumentNullException("sourceLabelSeriesSet");


      Interval = interval;
      ProfileGraphs = new ReadOnlyCollection<ProfileGraph>(profileGraphs);
      SourceLabelSeriesSet = sourceLabelSeriesSet;
    }

    public string Interval { get; private set; }
    public ICollection<ProfileGraph> ProfileGraphs { get; private set; }
    public LabelSeriesSet SourceLabelSeriesSet { get; private set; }

    public IList<DateTime> Categories { get; private set; }
    public LabelSeriesSet PreparedLabelSeriesSet { get; private set; }

    public void Prepare(ICollection<LabelObisCodeTemplate> labelObisCodeTemplates)
    {
      Categories = new ReadOnlyCollection<DateTime>(GetCategories());
      PreparedLabelSeriesSet = SourceLabelSeriesSet.Normalize(timeDivider);

      PreparedLabelSeriesSet.GenerateSeriesFromCumulative();
      PreparedLabelSeriesSet.GenerateFromTemplates(labelObisCodeTemplates);
    }

    private List<DateTime> GetCategories()
    {
      var categories = new List<DateTime>();
      var categoryTimestamp = SourceLabelSeriesSet.Start;
      while (categoryTimestamp < SourceLabelSeriesSet.End)
      {
        categories.Add(categoryTimestamp);
        categoryTimestamp = getNext(categoryTimestamp);
      }

      return categories;
    }
  }
}
