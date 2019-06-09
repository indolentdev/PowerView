using System;
namespace PowerView.Model
{
  public interface ISerieName : IEquatable<ISerieName>
  {
    string Label { get; }
    ObisCode ObisCode { get; }
  }
}
