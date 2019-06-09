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

    private readonly ISerieColorRepository serieColorRepository;
    private readonly ISerieNameRepository serieNameRepository;
    private readonly IObisColorProvider obisColorProvider;
    private readonly ITemplateConfigProvider templateConfigProvider;

    public SettingsSerieColorsModule(ISerieColorRepository serieColorRepository, ISerieNameRepository serieNameRepository, IObisColorProvider obisColorProvider, ITemplateConfigProvider templateConfigProvider)
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

      Get[""] = GetSerieColors;
      Put[""] = PutSerieColors;
    }

    private dynamic GetSerieColors(dynamic param)
    {
      var serieColorsDb = serieColorRepository.GetSerieColors();
      var serieColors = serieNameRepository.GetSerieNames(templateConfigProvider.LabelObisCodeTemplates)
        .ToDictionary(sn => sn, sn => new SerieColor(sn.Label, sn.ObisCode, obisColorProvider.GetColor(sn.ObisCode)));

      foreach (var serieColor in serieColorsDb)
      {
        serieColors[new SerieName(serieColor.Label, serieColor.ObisCode)] = serieColor;
      }

      var items = serieColors.Values.Select(sc => new SerieColorDto
      {
        Label = sc.Label, ObisCode = sc.ObisCode.ToString(), Color = sc.Color
      })
      .OrderBy(x => x.Label).ThenBy(x => x.ObisCode);
      var r = new SerieColorSetDto { Items = items.ToArray() };

      return Response.AsJson(r);
    }

    private dynamic PutSerieColors(dynamic param)
    {
      var serieColorSetDto = this.Bind<SerieColorSetDto>();

      var serieColors = ToSerieColors(serieColorSetDto.Items).ToArray();
      if (serieColors.Length > 0)
      {
        serieColorRepository.SetSerieColors(serieColors);
      }

      return Response.AsJson(string.Empty, HttpStatusCode.NoContent);
    }

    private static IEnumerable<SerieColor> ToSerieColors(IEnumerable<SerieColorDto> serieColorDtos)
    {
      foreach (var serieColorDto in serieColorDtos)
      {
        if (!SerieColor.IsColorValid(serieColorDto.Color))
        {
          log.InfoFormat("Skipping serie color item having invalid color format {0} {1} {2}",
            serieColorDto.Label, serieColorDto.ObisCode, serieColorDto.Color);
          continue;
        }

        yield return new SerieColor(serieColorDto.Label, serieColorDto.ObisCode, serieColorDto.Color);
      }
    }

  }
}
