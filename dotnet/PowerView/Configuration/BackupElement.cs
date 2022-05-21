using System.Configuration;

namespace PowerView.Configuration
{
  public class BackupElement : ConfigurationElement, IConfigurationValidatable
  {
    private const string MinimumIntervalDaysString = "MinimumIntervalDays";
    [ConfigurationProperty(MinimumIntervalDaysString)]
    public IntElement MinimumIntervalDays
    { 
      get { return (IntElement)this[MinimumIntervalDaysString];  }
      set { this[MinimumIntervalDaysString] = value; }
    }

    private const string MaximumCountString = "MaximumCount";
    [ConfigurationProperty(MaximumCountString)]
    public IntElement MaximumCount
    { 
      get { return (IntElement)this[MaximumCountString];  }
      set { this[MaximumCountString] = value; }
    }

    public void Validate()
    {
      MinimumIntervalDays.Validate(MinimumIntervalDaysString);
      MaximumCount.Validate(MaximumCountString);
    }

  }
}

