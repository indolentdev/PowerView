using System.Configuration;

namespace PowerView.Configuration
{
  public class PvOutputFacadeElement : ConfigurationElement, IConfigurationValidatable
  {
    private const string PvOutputAddStatusUrlString = "PvOutputAddStatusUrl";
    [ConfigurationProperty(PvOutputAddStatusUrlString)]
    public UriElement PvOutputAddStatusUrl
    { 
      get { return (UriElement)this[PvOutputAddStatusUrlString]; }
      set { this[PvOutputAddStatusUrlString] = value; }
    }

    private const string PvDeviceLabelString = "PvDeviceLabel";
    [ConfigurationProperty(PvDeviceLabelString)]
    public StringElement PvDeviceLabel
    { 
      get { return (StringElement)this[PvDeviceLabelString]; }
      set { this[PvDeviceLabelString] = value; }
    }

    private const string PvDeviceSerialNumberString = "PvDeviceSerialNumber";
    [ConfigurationProperty(PvDeviceSerialNumberString)]
    public StringElement PvDeviceSerialNumber
    { 
      get { return (StringElement)this[PvDeviceSerialNumberString]; }
      set { this[PvDeviceSerialNumberString] = value; }
    }

    private const string PvDeviceSerialNumberParamString = "PvDeviceSerialNumberParam";
    [ConfigurationProperty(PvDeviceSerialNumberParamString)]
    public StringElement PvDeviceSerialNumberParam
    { 
      get { return (StringElement)this[PvDeviceSerialNumberParamString]; }
      set { this[PvDeviceSerialNumberParamString] = value; }
    }

    private const string ActualPowerP23L1ParamString = "ActualPowerP23L1Param";
    [ConfigurationProperty(ActualPowerP23L1ParamString)]
    public StringElement ActualPowerP23L1Param
    {
      get { return (StringElement)this[ActualPowerP23L1ParamString]; }
      set { this[ActualPowerP23L1ParamString] = value; }
    }

    private const string ActualPowerP23L2ParamString = "ActualPowerP23L2Param";
    [ConfigurationProperty(ActualPowerP23L2ParamString)]
    public StringElement ActualPowerP23L2Param
    {
      get { return (StringElement)this[ActualPowerP23L2ParamString]; }
      set { this[ActualPowerP23L2ParamString] = value; }
    }

    private const string ActualPowerP23L3ParamString = "ActualPowerP23L3Param";
    [ConfigurationProperty(ActualPowerP23L3ParamString)]
    public StringElement ActualPowerP23L3Param
    {
      get { return (StringElement)this[ActualPowerP23L3ParamString]; }
      set { this[ActualPowerP23L3ParamString] = value; }
    }

    public void Validate()
    {
      if (string.IsNullOrEmpty(PvOutputAddStatusUrl.Value))
      {
        PvOutputAddStatusUrl.Value = "http://pvoutput.org/service/r2/addstatus.jsp";
      }
      if (string.IsNullOrEmpty(PvDeviceSerialNumber.Value))
      {
        PvDeviceSerialNumber.Value = null;
      }
      if (string.IsNullOrEmpty(PvDeviceSerialNumberParam.Value))
      {
        PvDeviceSerialNumberParam.Value = "v12";
      }

      PvOutputAddStatusUrl.Validate(PvOutputAddStatusUrlString);
      PvDeviceSerialNumberParam.Validate(PvDeviceSerialNumberParamString);

      if (string.IsNullOrEmpty(ActualPowerP23L1Param.Value))
      {
        ActualPowerP23L1Param.Value = "v7";
      }
      if (string.IsNullOrEmpty(ActualPowerP23L2Param.Value))
      {
        ActualPowerP23L2Param.Value = "v8";
      }
      if (string.IsNullOrEmpty(ActualPowerP23L3Param.Value))
      {
        ActualPowerP23L3Param.Value = "v9";
      }
    }

  }
}
