using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Nancy;
using Nancy.ModelBinding;

using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;

namespace PowerView.Service.Modules
{
  public class SettingsSerieColorsModule : CommonNancyModule
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly ISeriesColorRepository serieColorRepository;
    private readonly ISeriesNameRepository serieNameRepository;
    private readonly IObisColorProvider obisColorProvider;
    private readonly ITemplateConfigProvider templateConfigProvider;

    public SettingsSerieColorsModule(ISeriesColorRepository serieColorRepository, ISeriesNameRepository serieNameRepository, IObisColorProvider obisColorProvider, ITemplateConfigProvider templateConfigProvider)
      : base("/api/settings/seriecolors")
    {
      if (serieColorRepository == null) throw new ArgumentNullException("serieColorRepository");
      if (serieNameRepository == null) throw new ArgumentNullException("serieNameRepository");
      if (obisColorProvider == null) throw new ArgumentNullException("obisColorProvider");
      if (templateConfigProvider == null) throw new ArgumentNullException("templateConfigProvider");

      this.serieColorRepository = serieColorRepository;
      this.serieNameRepository = serieNameRepository;
      this.obisColorProvider = obisColorProvider;
      this.templateConfigProvider = templateConfigProvider;

      Get[""] = GetSeriesColors;
      Put[""] = PutSeriesColors;
    }

    private dynamic GetSeriesColors(dynamic param)
    {
      var seriesColorsDb = serieColorRepository.GetSeriesColors();
      var seriesColors = serieNameRepository.GetSeriesNames(templateConfigProvider.LabelObisCodeTemplates)
        .ToDictionary(sn => sn, sn => new SeriesColor(new SeriesName(sn.Label, sn.ObisCode), obisColorProvider.GetColor(sn.ObisCode)));

      foreach (var seriesColor in seriesColorsDb)
      {
        seriesColors[seriesColor.SeriesName] = seriesColor;
      }

      var items = seriesColors.Values.Select(sc => new SerieColorDto
      {
        Label = sc.SeriesName.Label, ObisCode = sc.SeriesName.ObisCode.ToString(), Color = sc.Color
      })
      .OrderBy(x => x.Label).ThenBy(x => x.ObisCode);
      var r = new SerieColorSetDto { Items = items.ToArray() };

      return Response.AsJson(r);
    }

    private dynamic PutSeriesColors(dynamic param)
    {
      var seriesColorSetDto = this.Bind<SerieColorSetDto>();

      var seriesColors = ToSeriesColors(seriesColorSetDto.Items).ToArray();
      if (seriesColors.Length > 0)
      {
        serieColorRepository.SetSeriesColors(seriesColors);
      }

      return Response.AsJson(string.Empty, HttpStatusCode.NoContent);
    }

    private static IEnumerable<SeriesColor> ToSeriesColors(IEnumerable<SerieColorDto> seriesColorDtos)
    {
      foreach (var seriesColorDto in seriesColorDtos)
      {
        if (!SeriesColor.IsColorValid(seriesColorDto.Color))
        {
          log.InfoFormat("Skipping serie color item having invalid color format {0} {1} {2}",
            seriesColorDto.Label, seriesColorDto.ObisCode, seriesColorDto.Color);
          continue;
        }

        yield return new SeriesColor(new SeriesName(seriesColorDto.Label, seriesColorDto.ObisCode), seriesColorDto.Color);
      }
    }

  }
}
