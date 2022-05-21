/*
using System.Configuration;

namespace PowerView.Configuration
{
  public class ServiceSection : ConfigurationSection, IConfigurationValidatable
  {
    private const string BaseUrlString = "BaseUrl";
    [ConfigurationProperty(BaseUrlString)]
    public UriElement BaseUrl
    {
      get { return (UriElement)this[BaseUrlString];  }
      set { this[BaseUrlString] = value; }
    }

    private const string PvOutputFacadeString = "PvOutputFacade";
    [ConfigurationProperty(PvOutputFacadeString)]
    public PvOutputFacadeElement PvOutputFacade
    { 
      get { return (PvOutputFacadeElement)this[PvOutputFacadeString];  }
      set { this[PvOutputFacadeString] = value; }
    }

    public void Validate()
    {
      BaseUrl.Validate(BaseUrlString);
      PvOutputFacade.Validate();
    }

  }
}
*/