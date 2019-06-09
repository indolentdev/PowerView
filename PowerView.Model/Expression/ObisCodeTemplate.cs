using System;
namespace PowerView.Model.Expression
{
  public class ObisCodeTemplate
  {
    public ObisCodeTemplate(ObisCode obisCode, ITemplateExpression templateExpression)
    {
      if (templateExpression == null) throw new ArgumentNullException("templateExpression");

      ObisCode = obisCode;
      TemplateExpression = templateExpression;
    }

    public ObisCode ObisCode { get; private set; }
    public ITemplateExpression TemplateExpression { get; private set; }
  }
}
