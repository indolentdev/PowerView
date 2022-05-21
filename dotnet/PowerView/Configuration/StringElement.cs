using System.Configuration;

namespace PowerView.Configuration
{
  public class StringElement : ConfigurationElement
  {
    private const string ValueString = "value";

    [ConfigurationProperty(ValueString, IsRequired = true, IsKey = false)]
    public string Value
    {
      get { return (string)this[ValueString]; }
      set { this[ValueString] = value; } 
    }

    public virtual void Validate(string attributeName)
    {
      if ( string.IsNullOrEmpty(Value) )
      {
        throw new ConfigurationErrorsException(attributeName + " value attribute is empty or absent");
      }
    }
  }
}

