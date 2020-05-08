using System;

namespace PowerView.Service.Modules
{
  public interface IPvOutputFacadeModuleConfigProvider
  {
    Uri PvOutputAddStatus { get; }
    string PvDeviceLabel { get; }
    string PvDeviceId { get; }
    string PvDeviceIdParam { get; }

    string ActualPowerP23L1Param { get; }
    string ActualPowerP23L2Param { get; }
    string ActualPowerP23L3Param { get; }
  }
}

