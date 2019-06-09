using System;
using System.Collections.Generic;
using System.IO;
using PowerView.Model;

namespace PowerView.Service.Mappers
{
  public interface ILiveReadingMapper
  {
    IEnumerable<LiveReading> Map(string contentType, Stream body);

    LiveReading MapPvOutputArgs(Uri requestUrl, string contentType, Stream body, string deviceLabel, string deviceSerialNumber, string deviceSerialNumberParam, string actualPowerP23L1Param, string actualPowerP23L2Param, string actualPowerP23L3Param);
  }
}
