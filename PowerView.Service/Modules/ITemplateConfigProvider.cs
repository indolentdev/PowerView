using System.Collections.Generic;
using PowerView.Model;
using PowerView.Model.Expression;

namespace PowerView.Service.Modules
{
  public interface ITemplateConfigProvider
  {
    ICollection<LabelObisCodeTemplate> LabelObisCodeTemplates { get; }
  }
}

