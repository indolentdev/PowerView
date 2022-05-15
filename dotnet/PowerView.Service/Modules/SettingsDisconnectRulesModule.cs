/*
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Extensions;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;
using PowerView.Service.Mappers;

namespace PowerView.Service.Modules
{
  public class SettingsDisconnectRulesModule : CommonNancyModule
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IDisconnectRuleRepository disconnectRuleRepository;
    private readonly IDisconnectRuleMapper disconnectRuleMapper;

    public SettingsDisconnectRulesModule(IDisconnectRuleRepository disconnectRuleRepository, IDisconnectRuleMapper disconnectRuleMapper)
      : base("/api/settings/disconnectrules")
    {
      if (disconnectRuleRepository == null) throw new ArgumentNullException("disconnectRuleRepository");
      if (disconnectRuleMapper == null) throw new ArgumentNullException("disconnectRuleMapper");

      this.disconnectRuleRepository = disconnectRuleRepository;
      this.disconnectRuleMapper = disconnectRuleMapper;

      Get[""] = GetDisconnectRules;
      Get["options"] = GetDisconnectControlOptions;

      Post[""] = AddDisconnectRule;
      Delete["names/{label}/{obisCode}"] = DeleteDisconnectRule;
    }

    private dynamic GetDisconnectRules(dynamic param)
    {
      var disconnectRules = disconnectRuleRepository.GetDisconnectRules();

      var disconnectRuleDtos = disconnectRules.Select(disconnectRuleMapper.MapToDto).ToArray();
      var disconnectRuleSetDto = new DisconnectRuleSetDto { Items = disconnectRuleDtos };

      return Response.AsJson(disconnectRuleSetDto);
    }


    private dynamic GetDisconnectControlOptions(dynamic param)
    {
      var latestSerieNames = disconnectRuleRepository.GetLatestSerieNames(DateTime.UtcNow);

      var disconnectRules = disconnectRuleRepository.GetDisconnectRules();
      var disconnectControlNames = latestSerieNames.Keys.Where(x => x.ObisCode.IsDisconnectControl)
                                               .Except(disconnectRules.Select(x => x.Name)).ToList();

      var evaluationObisCodes = new[] { ObisCode.ElectrActualPowerP23, ObisCode.ElectrActualPowerP23L1, ObisCode.ElectrActualPowerP23L2, ObisCode.ElectrActualPowerP23L3 };
      var evaluationItems = latestSerieNames.Where(x => evaluationObisCodes.Contains(x.Key.ObisCode)).ToList();

      var disconnectControlNameDtos = disconnectControlNames.Select(x => new { Label = x.Label, ObisCode = x.ObisCode.ToString() }).ToList();
      var evaluationItemDtos = evaluationItems.Select(x => new { Label = x.Key.Label, ObisCode = x.Key.ObisCode.ToString(), 
        Unit = ValueAndUnitMapper.Map(x.Value, false) });
      return Response.AsJson(new { DisconnectControlItems = disconnectControlNameDtos, EvaluationItems = evaluationItemDtos });
    }

    private dynamic AddDisconnectRule(dynamic param)
    {
      var disconnectRuleDto = this.Bind<DisconnectRuleDto>();
      DisconnectRule disconnectRule;
      try
      {
        disconnectRule = disconnectRuleMapper.MapFromDto(disconnectRuleDto);
      }
      catch (ArgumentException e)
      {
        log.Warn("Add disconnect rule failed. Invalid content. Json:" + Request.Body.AsString(), e);
        return Response.AsJson(new { Description = "Disconnect rule content invalid" }, HttpStatusCode.UnsupportedMediaType);
      }

      try
      {
        disconnectRuleRepository.AddDisconnectRule(disconnectRule);
      }
      catch (DataStoreUniqueConstraintException e)
      {
        log.Warn("Add disconnect rule failed. Unique constraint violation. Json:" + Request.Body.AsString(), e);
        return Response.AsJson(new { Description = "Disconnect rule unique constraint violation" }, HttpStatusCode.Conflict);
      }

      log.InfoFormat("Inserted/Updated DisconnectRule; Label:{0},ObisCode:{1}", disconnectRule.Name.Label, disconnectRule.Name.ObisCode);

      return HttpStatusCode.NoContent;
    }

    private dynamic DeleteDisconnectRule(dynamic param)
    {
      string label = param.label;
      string obisCode = param.obisCode;

      ISeriesName name;
      try
      {
        name = new SeriesName(label, obisCode);
      }
      catch (ArgumentException e)
      {
        log.Warn("Delete disconnect rule failed. Bad name. Label:" + label + ", ObisCode:" + obisCode, e);
        return Response.AsJson(new { Description = "Disconnect rule name invalid" }, HttpStatusCode.UnsupportedMediaType);
      }

      disconnectRuleRepository.DeleteDisconnectRule(name);

      log.InfoFormat("Deleted DisconnectRule; Label:{0},ObisCode:{1}", name.Label, name.ObisCode);

      return HttpStatusCode.NoContent;
    }

  }
}
*/