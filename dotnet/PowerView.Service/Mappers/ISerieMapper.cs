using System;
using PowerView.Model;

namespace PowerView.Service.Mappers
{
  public interface ISerieMapper
  {
    string MapToSerieType(ObisCode obisCode);

    string MapToSerieYAxis(ObisCode obisCode);
  }
}

