using System;

namespace PowerView.Service.Modules
{
  public interface IPvOutputFacadeModuleConfigProvider
  {
    Uri PvOutputAddStatus { get; }
    string PvDeviceLabel { get; }
    string PvDeviceSerialNumber { get; }
    string PvDeviceSerialNumberParam { get; }

    string ActualPowerP23L1Param { get; }
    string ActualPowerP23L2Param { get; }
    string ActualPowerP23L3Param { get; }
  }
}

