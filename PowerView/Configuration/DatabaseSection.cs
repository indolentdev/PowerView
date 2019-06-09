using System.Configuration;

namespace PowerView.Configuration
{
  public class DatabaseSection : ConfigurationSection, IConfigurationValidatable
  {
    private const string NameString = "Name";
    [ConfigurationProperty(NameString)]
    public StringElement Name
    {
      get { return (StringElement)this[NameString];  }
      set { this[NameString] = value; }
    }

    private const string BackupString = "Backup";
    [ConfigurationProperty(BackupString)]
    public BackupElement Backup
    { 
      get { return (BackupElement)this[BackupString];  }
      set { this[BackupString] = value; }
    }

    public bool HasBackupElement { get { return Backup != null; } }

    private const string TimeZoneString = "TimeZone";
    [ConfigurationProperty(TimeZoneString)]
    public StringElement TimeZone
    {
      get { return (StringElement)this[TimeZoneString];  }
      set { this[TimeZoneString] = value; }
    }

    private const string CultureInfoString = "CultureInfo";
    [ConfigurationProperty(CultureInfoString)]
    public StringElement CultureInfo
    {
      get { return (StringElement)this[CultureInfoString]; }
      set { this[CultureInfoString] = value; }
    }

    private const string IntegrityCheckCommandTimeoutString = "IntegrityCheckCommandTimeout";
    [ConfigurationProperty(IntegrityCheckCommandTimeoutString)]
    public IntElement IntegrityCheckCommandTimeout
    {
      get { return (IntElement)this[IntegrityCheckCommandTimeoutString]; }
      set { this[IntegrityCheckCommandTimeoutString] = value; }
    }

    public void Validate()
    {
      Name.Validate(NameString);
      if (HasBackupElement)
      {
        Backup.Validate();
      }

      if (string.IsNullOrEmpty(IntegrityCheckCommandTimeout.Value))
      {
        IntegrityCheckCommandTimeout.Value = "600";
      }

    }

  }
}

