using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using PowerView.Model.Expression;
using log4net;

namespace PowerView.Model
{
  public class LabelSeriesFromTemplatesGenerator
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly ICollection<LabelObisCodeTemplate> labelObisCodeTemplates;

    internal LabelSeriesFromTemplatesGenerator(ICollection<LabelObisCodeTemplate> labelObisCodeTemplates)
    {
      if (labelObisCodeTemplates == null) throw new ArgumentNullException("labelObisCodeTemplates");
      this.labelObisCodeTemplates = labelObisCodeTemplates;
    }

    public IList<LabelSeries<NormalizedTimeRegisterValue>> Generate(LabelSeriesSet<NormalizedTimeRegisterValue> labelSeriesSet)
    {
      var labelsAndObisCodes = GetLabelsAndObisCodes(labelSeriesSet);

      var labelSeries = new List<LabelSeries<NormalizedTimeRegisterValue>>();
      foreach (var labelObisCodeTemplate in labelObisCodeTemplates)
      {
        var obisCodeAndValues = Generate(labelSeriesSet, labelsAndObisCodes, labelObisCodeTemplate.Label, labelObisCodeTemplate.ObisCodeTemplates);
        if (obisCodeAndValues.Count == 0)
        {
          continue;
        }
        var labelSeriesItem = new LabelSeries<NormalizedTimeRegisterValue>(labelObisCodeTemplate.Label, obisCodeAndValues);
        labelSeries.Add(labelSeriesItem);
      }
      return labelSeries;
    }

    private IDictionary<string, ICollection<ObisCode>> GetLabelsAndObisCodes(LabelSeriesSet<NormalizedTimeRegisterValue> labelSeriesSet)
    {
      var result = new Dictionary<string, ICollection<ObisCode>>();

      foreach (var labelSeries in labelSeriesSet)
      {
        result.Add(labelSeries.Label.ToLowerInvariant(), labelSeries.ToList());
      }

      return result;
    }

    private IDictionary<ObisCode, IEnumerable<NormalizedTimeRegisterValue>> Generate(LabelSeriesSet<NormalizedTimeRegisterValue> labelSeriesSet, IDictionary<string, ICollection<ObisCode>> labelsAndObisCodes, string label, ICollection<ObisCodeTemplate> obisCodeTemplates)
    {
      var obisCodeAndValues = new Dictionary<ObisCode, IEnumerable<NormalizedTimeRegisterValue>>();
      foreach (var obisCodeTemplate in obisCodeTemplates)
      {
        if (!obisCodeTemplate.TemplateExpression.IsSatisfied(labelsAndObisCodes))
        {
          log.DebugFormat("Template has unmet data sources. Skipping it. Label:{0}, ObisCode:{1}", label, obisCodeTemplate.ObisCode);
          continue;
        }

        try
        {
          var valueExpressionSet = obisCodeTemplate.TemplateExpression.GetValueExpressionSet(labelSeriesSet);
          var normalizedTimeRegisterValues = valueExpressionSet.Evaluate2();
          obisCodeAndValues.Add(obisCodeTemplate.ObisCode, normalizedTimeRegisterValues);
        }
        catch (ValueExpressionSetException e)
        {
          var msg = string.Format(CultureInfo.InvariantCulture, "Failed calculation for template. Skipping it. Label:{0}, ObisCode:{1}",
                                  label, obisCodeTemplate.ObisCode);
          log.Error(msg, e);
        }
      }
      return obisCodeAndValues;
    }

  }

}
