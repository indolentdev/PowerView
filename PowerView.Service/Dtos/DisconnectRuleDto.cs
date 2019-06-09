
namespace PowerView.Service.Dtos
{
  public class DisconnectRuleDto
  {
    public string NameLabel { get; set; }
    public string NameObisCode { get; set; }
    public string EvaluationLabel { get; set; }
    public string EvaluationObisCode { get; set; }
    public int DurationMinutes { get; set; }
    public int DisconnectToConnectValue { get; set; }
    public int ConnectToDisconnectValue { get; set; }
    public string Unit { get; set; }
  }
}
