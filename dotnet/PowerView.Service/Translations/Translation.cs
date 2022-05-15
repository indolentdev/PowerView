using System;
using System.Globalization;
using System.Resources;
using PowerView.Model;

namespace PowerView.Service.Translations
{
  internal class Translation : ITranslation
  {
    private readonly Lazy<ResourceManager> mailRm = new Lazy<ResourceManager>(
      () => new ResourceManager("PowerView.Service.Translations.Mail", typeof(Translation).Assembly));

    private readonly CultureInfo cultureInfo;
    
    public Translation(ILocationContext locationContext)
    {
      cultureInfo = locationContext.CultureInfo;
    }

    public string Get(ResId resId)
    {
      return GetString(resId);
    }

    public string Get(ResId resId, object arg1)
    {
      var resource = GetString(resId);
      return string.Format(cultureInfo, resource, arg1);
    }

    public string Get(ResId resId, object arg1, object arg2)
    {
      var resource = GetString(resId);
      return string.Format(cultureInfo, resource, arg1, arg2);
    }

    public string Get(ResId resId, object arg1, object arg2, object arg3)
    {
      var resource = GetString(resId);
      return string.Format(cultureInfo, resource, arg1, arg2, arg3);
    }

    public string Get(ResId resId, params object[] args)
    {
      var resource = GetString(resId);
      return string.Format(cultureInfo, resource, args);
    }

    private string GetString(ResId resId)
    {
      var resource = GetResourceManager(resId).GetString(resId.ToString(), cultureInfo);
      return resource;
    }

    private ResourceManager GetResourceManager(ResId resId)
    {
      return mailRm.Value;
    }
  }
}
