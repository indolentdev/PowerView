using System;

namespace PowerView.Service.Dtos
{
  public class RegisterValueDto
  {
    public string ObisCode { get; set; }
    public int Value { get; set; }
    public short Scale { get; set; }
    public string Unit { get; set; }
  }
}

