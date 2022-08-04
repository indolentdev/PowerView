using System.ComponentModel.DataAnnotations;

namespace PowerView.Service;

public class ObisCodeAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is string str && PowerView.Model.ObisCode.TryParse(str, out _))
        {
            return true;
        }

        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be an OBIS code";
    }
}
