using System.Configuration;

namespace PowerView.Configuration
{
  public class RegisterSection : ConfigurationSection, IConfigurationValidatable
  {
    private const string CalculationsString = "Calculations";
    [ConfigurationProperty(CalculationsString, IsRequired = true)]
    [ConfigurationCollection(typeof(CalculationElementCollection), AddItemName = "Calculation", ClearItemsName = "Clear", RemoveItemName = "Remove")]
    public CalculationElementCollection Calculations
    { 
      get { return (CalculationElementCollection)this[CalculationsString];  }
      set { this[CalculationsString] = value; }
    }

    public void Validate()
    {
      Calculations.Validate();
    }

  }
}

