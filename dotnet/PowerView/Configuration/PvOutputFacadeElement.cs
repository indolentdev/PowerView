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

    private const string PvDeviceIdString = "PvDeviceId";
    [ConfigurationProperty(PvDeviceIdString)]
    public StringElement PvDeviceId
    { 
      get { return (StringElement)this[PvDeviceIdString]; }
      set { this[PvDeviceIdString] = value; }
    }

    private const string PvDeviceIdParamString = "PvDeviceIdParam";
    [ConfigurationProperty(PvDeviceIdParamString)]
    public StringElement PvDeviceIdParam
    { 
      get { return (StringElement)this[PvDeviceIdParamString]; }
      set { this[PvDeviceIdParamString] = value; }
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
      if (string.IsNullOrEmpty(PvDeviceId.Value))
      {
        PvDeviceId.Value = null;
      }
      if (string.IsNullOrEmpty(PvDeviceIdParam.Value))
      {
        PvDeviceIdParam.Value = "v12";
      }

      PvOutputAddStatusUrl.Validate(PvOutputAddStatusUrlString);
      PvDeviceIdParam.Validate(PvDeviceIdParamString);

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
