using System.Configuration;
using System.Linq;

namespace PowerView.Configuration
{
  internal class PowerViewConfiguration : IPowerViewConfiguration
  {
    private readonly System.Configuration.Configuration configuration;

    public PowerViewConfiguration()
    {
      configuration = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
    }
    
    public ServiceSection GetServiceSection()
    {
      return GetSection<ServiceSection>("Service");
    }

    public DatabaseSection GetDatabaseSection()
    {
      return GetSection<DatabaseSection>("Database");
    }

    public RegisterSection GetRegisterSection()
    {
      return GetSection<RegisterSection>("Register");
    }

    private TSection GetSection<TSection>(string sectionName) where TSection : ConfigurationSection, IConfigurationValidatable
    {
      var section = (TSection)configuration.GetSection(sectionName);
      if (section == null)
      {
        throw new ConfigurationErrorsException(sectionName + " element is missing in the configuration");
      } 

      section.Validate();

      return section;
    }

  }
}

