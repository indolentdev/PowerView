using System;
using System.Collections.Generic;
using System.IO;
using PowerView.Model;

namespace PowerView.Service.Mappers
{
    public interface ILiveReadingMapper
    {
        Reading MapPvOutputArgs(Uri requestUrl, string contentType, Stream body, string deviceLabel, string deviceId, string deviceIdParam, string actualPowerP23L1Param, string actualPowerP23L2Param, string actualPowerP23L3Param);
    }
}
