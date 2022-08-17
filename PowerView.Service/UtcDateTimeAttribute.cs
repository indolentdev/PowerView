using System.ComponentModel.DataAnnotations;

namespace PowerView.Service;

public class UtcDateTimeAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is DateTime dateTime && dateTime.Kind == DateTimeKind.Utc)
        {
            return true;
        }

        return false;
    }
    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be a UTC date time";
    }
}
