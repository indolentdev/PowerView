/*
using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Nancy;
using Nancy.ModelBinding;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;

namespace PowerView.Service.Modules
{
  public class SettingsSmtpModule : CommonNancyModule
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly ISettingRepository settingRepository;

    public SettingsSmtpModule(ISettingRepository settingRepository)
      : base("/api/settings/smtp")
    {
      if (settingRepository == null) throw new ArgumentNullException("settingRepository");

      this.settingRepository = settingRepository;

      Get[""] = GetSettings;
      Put[""] = PutSettings;
    }

    private dynamic GetSettings(dynamic param)
    {
      SmtpConfig smtpConfig;
      try
      {
        smtpConfig = settingRepository.GetSmtpConfig();
      }
      catch (DomainConstraintException)
      {
        return Response.AsJson(new { });
      }

      var r = new { Server = smtpConfig.Server, Port = smtpConfig.Port.ToString(CultureInfo.InvariantCulture),
        User = smtpConfig.User, Auth = smtpConfig.Auth };
      return Response.AsJson(r);
    }

    private dynamic PutSettings(dynamic param)
    {
      var smtpConfig = GetSmtpConfig();
      if (smtpConfig == null)
      {
        var description = new { Description = "Server, Port, User or Auth properties absent, empty or invalid" };
        return Response.AsJson(description, HttpStatusCode.UnsupportedMediaType);
      }

      settingRepository.UpsertSmtpConfig(smtpConfig);
      return HttpStatusCode.NoContent;
    }

    private SmtpConfig GetSmtpConfig()
    {
      var smtpConfigDto = this.Bind<SmtpConfigDto>();
        
      if (string.IsNullOrEmpty(smtpConfigDto.Server) || smtpConfigDto.Port == null || string.IsNullOrEmpty(smtpConfigDto.User) || string.IsNullOrEmpty(smtpConfigDto.Auth))
      {
        log.WarnFormat("Set SMTP configuration failed. Properties are null or empty. Server:{0}, Port:{1}, User:{2}, Auth:********",
                       smtpConfigDto.Server, smtpConfigDto.Port, smtpConfigDto.User);
        return null;
      }

      ushort port;
      if (!ushort.TryParse(smtpConfigDto.Port, NumberStyles.Integer, CultureInfo.InvariantCulture, out port)) return null;
      if (port < 1) return null;

      return new SmtpConfig(smtpConfigDto.Server, port, smtpConfigDto.User, smtpConfigDto.Auth);
    }

  }
}
*/
