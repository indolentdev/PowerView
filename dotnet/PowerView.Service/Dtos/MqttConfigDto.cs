
namespace PowerView.Service.Dtos
{
  public class MqttConfigDto
  {
    public bool? PublishEnabled { get; set; }
    public string Server { get; set; }
    public ushort Port { get; set; }
    public string ClientId { get; set; }
  }
}
