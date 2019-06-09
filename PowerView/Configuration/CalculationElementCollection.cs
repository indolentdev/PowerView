using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using PowerView.Model.Expression;

namespace PowerView.Configuration
{
  public class CalculationElementCollection : ConfigurationElementCollection, IConfigurationValidatable
  {
    protected override ConfigurationElement CreateNewElement()
    {
      return new CalculationElement();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      var calculationElement = (CalculationElement)element;
      return calculationElement.Label + ":" + calculationElement.ObisCode;
    }

    public void Validate()
    {
      foreach (IConfigurationValidatable element in this)
      {
        element.Validate();
      }
    }

    public ICollection<LabelObisCodeTemplate> GetLabelObisCodeTemplates()
    {
      var labelGroupings = this.Cast<CalculationElement>().GroupBy(ce => ce.Label, ce => ce);

      var result = new List<LabelObisCodeTemplate>();
      foreach (var grouping in labelGroupings)
      {
        result.Add(new LabelObisCodeTemplate(grouping.Key, grouping.Select(x => new ObisCodeTemplate(x.GetObisCode(), x.GetTemplateExpression()))));
      }
      return result;
    }

    internal void Add(CalculationElement element)
    {
      LockItem = false;
      BaseAdd(element);
    }
  }
}

