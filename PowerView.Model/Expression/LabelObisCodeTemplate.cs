using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model.Expression
{
  public class LabelObisCodeTemplate
  {
    public LabelObisCodeTemplate(string label, IEnumerable<ObisCodeTemplate> obisCodeTemplates)
    {
      if (string.IsNullOrEmpty(label)) throw new ArgumentNullException("label");
      if (obisCodeTemplates == null) throw new ArgumentNullException("obisCodeTemplates");
      var obisCodeTemplatesLocal = obisCodeTemplates.ToList();
      if (obisCodeTemplatesLocal.Any(x => x.TemplateExpression == null)) throw new ArgumentNullException("obisCodeTemplates", "ObisCodeTemplate items must not be null");

      Label = label;
      ObisCodeTemplates = obisCodeTemplatesLocal;
    }

    public string Label { get; private set; }
    public ICollection<ObisCodeTemplate> ObisCodeTemplates { get; private set; }
  }
}
