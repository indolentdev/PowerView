using System;
using System.Collections.Generic;
using PowerView.Model.Expression;

namespace PowerView.Service.Modules
{
  internal class TemplateConfigProvider : ITemplateConfigProvider
  {
    public TemplateConfigProvider(ICollection<LabelObisCodeTemplate> labelObisCodeTemplates)
    {
      if (labelObisCodeTemplates == null) throw new ArgumentNullException("labelObisCodeTemplates");

      LabelObisCodeTemplates = labelObisCodeTemplates;
    }

    public ICollection<LabelObisCodeTemplate> LabelObisCodeTemplates { get; private set; }
  }
}
