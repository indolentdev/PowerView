﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PowerView.Model.Expression
{
  internal class RegisterTemplateExpression : ITemplateExpression
  {
//    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public string Label { get; private set; }
    public ObisCode ObisCode { get; private set; }
    
    public RegisterTemplateExpression(string template)
    {
      if (string.IsNullOrEmpty(template)) throw new ArgumentNullException("template");

      var parts = template.Split(new [] {':'});
      if (parts.Length != 2)
      {
        var msg = string.Format(CultureInfo.InvariantCulture, "Unable to parse template:{0}", template);
        throw new TemplateExpressionException(msg);
      }

      var oc = GetObisCode(parts[1]);

      Label = parts[0];
      ObisCode = oc;
    }

    private static ObisCode GetObisCode(string oc)
    {
      try 
      {
        return (ObisCode)oc;
      }
      catch (ArgumentException e)
      {
        var msg = string.Format(CultureInfo.InvariantCulture, "Unable to parse ObisCode:{0}", oc);
        throw new TemplateExpressionException(msg, e);
      }
    }

    public bool IsSatisfied(IDictionary<string, ICollection<ObisCode>> labelsAndObisCodes)
    {
      if (labelsAndObisCodes == null) throw new ArgumentNullException("labelsAndObisCodes");

      var label = Label.ToLowerInvariant();
      if (!labelsAndObisCodes.ContainsKey(label))
      {
        return false;
      }

      var obisCodes = labelsAndObisCodes[label];
      if (obisCodes == null || !obisCodes.Contains(ObisCode))
      {
        return false;
      }

      return true;
    }

    public IValueExpressionSet GetValueExpressionSet(LabelSeriesSet<NormalizedTimeRegisterValue> labelSeriesSet)
    {
      var labelSeries = labelSeriesSet.FirstOrDefault(ls => string.Equals(ls.Label, Label, StringComparison.InvariantCultureIgnoreCase));
      if (labelSeries == null)
      {
        throw new ValueExpressionSetException("Unable to construct. " + Label + " not found");
      }

      var timeRegisterValues = labelSeries[ObisCode];
      return new ValueExpressionSet(timeRegisterValues);
    }

  }
}

