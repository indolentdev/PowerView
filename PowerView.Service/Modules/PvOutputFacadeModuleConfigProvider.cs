using System;

namespace PowerView.Service.Modules
{
  internal class PvOutputFacadeModuleConfigProvider : IPvOutputFacadeModuleConfigProvider
  {
    public PvOutputFacadeModuleConfigProvider(Uri pvOutputAddStatus, string pvDeviceLabel, string pvDeviceSerialNumber, string pvDeviceSerialNumberParam, 
                                              string actualPowerP23L1Param, string actualPowerP23L2Param, string actualPowerP23L3Param)
    {
      if (pvOutputAddStatus == null) throw new ArgumentNullException("pvOutputAddStatus");
      if (pvDeviceLabel == null) throw new ArgumentNullException("pvDeviceLabel");

      PvOutputAddStatus = pvOutputAddStatus;
      PvDeviceLabel = pvDeviceLabel;
      PvDeviceSerialNumber = pvDeviceSerialNumber;
      PvDeviceSerialNumberParam = pvDeviceSerialNumberParam;

      ActualPowerP23L1Param = actualPowerP23L1Param;
      ActualPowerP23L2Param = actualPowerP23L2Param;
      ActualPowerP23L3Param = actualPowerP23L3Param;
    }

    public Uri PvOutputAddStatus { get; private set; }
    public string PvDeviceLabel { get; private set; }
    public string PvDeviceSerialNumber { get; private set; }
    public string PvDeviceSerialNumberParam { get; private set; }

    public string ActualPowerP23L1Param { get; }
    public string ActualPowerP23L2Param { get; }
    public string ActualPowerP23L3Param { get; }
  }
}
