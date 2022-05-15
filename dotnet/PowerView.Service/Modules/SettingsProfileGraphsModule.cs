/*
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;

namespace PowerView.Service.Modules
{
  public class SettingsProfileGraphsModule : CommonNancyModule
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly ISeriesNameRepository serieNameRepository;
    private readonly IProfileGraphRepository profileGraphRepository;
    private readonly ILocationContext locationContext;

    public SettingsProfileGraphsModule(ISeriesNameRepository serieNameRepository, IProfileGraphRepository profileGraphRepository, ILocationContext locationContext)
      : base("/api/settings/profilegraphs")
    {
      if (serieNameRepository == null) throw new ArgumentNullException("serieNameRepository");
      if (profileGraphRepository == null) throw new ArgumentNullException("profileGraphRepository");
      if (locationContext == null) throw new ArgumentNullException("locationContext");

      this.serieNameRepository = serieNameRepository;
      this.profileGraphRepository = profileGraphRepository;
      this.locationContext = locationContext;

      Get["series"] = GetProfileGraphSeries;
      Get["pages"] = GetProfileGraphPages;
      Get[""] = GetProfileGraphs;
      Post[""] = PostProfileGraph;
      Put["modify/{existingProfileGraphIdBase64}"] = PutProfileGraph;
      Delete[""] = DeleteProfileGraph;
      Put["swaprank"] = SwapProfileGraphRank;
    }

    private dynamic GetProfileGraphSeries(dynamic param)
    {
      var timeZoneInfo = locationContext.TimeZoneInfo;
      var serieNames = serieNameRepository.GetSeriesNames(timeZoneInfo);

      var day = serieNames.Where(sn => !sn.ObisCode.IsDelta || sn.ObisCode == ObisCode.ElectrActiveEnergyA14NetDelta || sn.ObisCode == ObisCode.ElectrActiveEnergyA23NetDelta)
        .Select(sn => new { Period = "day", sn.Label, ObisCode = sn.ObisCode.ToString() });
      var month = serieNames.Where(sn => sn.ObisCode.IsDelta || sn.ObisCode.IsPeriod)
        .Select(sn => new { Period = "month", sn.Label, ObisCode = sn.ObisCode.ToString() });
      var year = serieNames.Where(sn => sn.ObisCode.IsDelta || sn.ObisCode.IsPeriod)
        .Select(sn => new { Period = "year", sn.Label, ObisCode = sn.ObisCode.ToString() });

      var r = new { Items = day.Concat(month).Concat(year) };
      return Response.AsJson(r);
    }

    private dynamic GetProfileGraphPages(dynamic param)
    {
      string period = Request.Query.period;

      var pages = profileGraphRepository.GetProfileGraphPages(period);

      var r = new { Items = pages };
      return Response.AsJson(r);
    }

    private dynamic GetProfileGraphs(dynamic param)
    {
      var profileGraphs = profileGraphRepository.GetProfileGraphs();

      var r = new 
      {
        Items = profileGraphs.Select(ToProfileGraphDto).ToList()
      };

      return Response.AsJson(r);
    }

    private static ProfileGraphDto ToProfileGraphDto(ProfileGraph profileGraph)
    {
      return new ProfileGraphDto
      {
        Title = profileGraph.Title, Page = profileGraph.Page, Period = profileGraph.Period,
        Interval = profileGraph.Interval, Rank = profileGraph.Rank, Series = profileGraph.SerieNames.Select(sn =>
          new ProfileGraphSerieDto { Label = sn.Label, ObisCode = sn.ObisCode.ToString() } ).ToArray()
      };
    }

    private dynamic PostProfileGraph(dynamic param)
    {
      var dto = this.Bind<ProfileGraphDto>();

      ProfileGraph profileGraph = null;
      try
      {
        var serieNames = dto.Series.Select(x => new SeriesName(x.Label, x.ObisCode)).ToList();
        profileGraph = new ProfileGraph(dto.Period, dto.Page, dto.Title, dto.Interval, 0, serieNames);
      }
      catch (ArgumentException e)
      {
        log.Warn("Add profile graph failed", e);
        return HttpStatusCode.UnsupportedMediaType;
      }

      try
      {
        profileGraphRepository.AddProfileGraph(profileGraph);
      }
      catch (DataStoreUniqueConstraintException e)
      {
        var msg = string.Format(CultureInfo.InvariantCulture, "Add profile graph failed. Period:{0}, Page:{1}, Title:{2}", 
                                dto.Period, dto.Page, dto.Title);
        log.Warn(msg, e);
        return Response.AsJson(new { Description = "ProfileGraph [period, page, title] or [period, page, rank] already exists" }, HttpStatusCode.Conflict);
      }
      return HttpStatusCode.NoContent;
    }

    private dynamic PutProfileGraph(dynamic param)
    {
      string existingProfileGraphIdBase64 = param.existingProfileGraphIdBase64;
      UpdateProfileGraphId updateProfileGraphId;
      try
      {
        var existingProfileGraphIdJson = Encoding.ASCII.GetString(Convert.FromBase64String(existingProfileGraphIdBase64));
        updateProfileGraphId = JsonConvert.DeserializeObject<UpdateProfileGraphId>(existingProfileGraphIdJson);
      }
      catch (FormatException e)
      {
        var msg = string.Format(CultureInfo.InvariantCulture, "Update profile graph failed. Unable to decode existing profile graph id:{0}",
                                  existingProfileGraphIdBase64);
        log.Warn(msg, e);
        return Response.AsJson(new { Description = "ProfileGraph [period, page, title] unknown" }, HttpStatusCode.UnsupportedMediaType);
      }
      catch (JsonException e)
      {
        var msg = string.Format(CultureInfo.InvariantCulture, "Update profile graph failed. Unable to decode existing profile graph id:{0}",
                                  existingProfileGraphIdBase64);
        log.Warn(msg, e);
        return Response.AsJson(new { Description = "ProfileGraph [period, page, title] unknown" }, HttpStatusCode.UnsupportedMediaType);
      }

      var dto = this.Bind<ProfileGraphDto>();
      ProfileGraph profileGraph = null;
      try
      {
        var serieNames = dto.Series.Select(x => new SeriesName(x.Label, x.ObisCode)).ToList();
        profileGraph = new ProfileGraph(dto.Period, dto.Page, dto.Title, dto.Interval, 0, serieNames);
      }
      catch (ArgumentException e)
      {
        log.Warn("Update profile graph failed.", e);
        return HttpStatusCode.UnsupportedMediaType;
      }

      var success = profileGraphRepository.UpdateProfileGraph(updateProfileGraphId.Period, updateProfileGraphId.Page, updateProfileGraphId.Title, profileGraph);
      if (!success)
      {
        var msg = string.Format(CultureInfo.InvariantCulture, "Update profile graph failed. Period:{0}, Page:{1}, Title:{2}. Does not exist.",
                                  updateProfileGraphId.Period, updateProfileGraphId.Page, updateProfileGraphId.Title);
        log.Warn(msg);
        return Response.AsJson(new { Description = "ProfileGraph [period, page, title] does not exist" }, HttpStatusCode.Conflict);
      }

      return HttpStatusCode.NoContent;
    }

    private class UpdateProfileGraphId
    {
      public string Period { get; set; }
      public string Page { get; set; }
      public string Title { get; set; }
    }

    private dynamic DeleteProfileGraph(dynamic param)
    {
      string period = Request.Query.period;
      string page = Request.Query.page;
      string title = Request.Query.title;

      profileGraphRepository.DeleteProfileGraph(period, page, title);

      return HttpStatusCode.NoContent;
    }

    private dynamic SwapProfileGraphRank(dynamic param)
    {
      string period = Request.Query.period;
      string page = Request.Query.page;
      string title1 = Request.Query.title1;
      string title2 = Request.Query.title2;

      try
      {
        profileGraphRepository.SwapProfileGraphRank(period, page, title1, title2);
      }
      catch (DataStoreUniqueConstraintException e)
      {
        var msg = string.Format(CultureInfo.InvariantCulture, "Swap profile graph rank. Period:{0}, Page:{1}, Title1:{2}, Title2:{3}",
                                period, page, title1, title2);
        log.Warn(msg, e);
        return Response.AsJson(new { Description = "ProfileGraph [period, page, rank] already exists" }, HttpStatusCode.Conflict);
      }
      return HttpStatusCode.NoContent;
    }

  }
}
*/