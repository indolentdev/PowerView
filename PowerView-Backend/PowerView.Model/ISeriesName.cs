using System;
namespace PowerView.Model
{
    public interface ISeriesName : IEquatable<ISeriesName>
    {
        string Label { get; }
        ObisCode ObisCode { get; }
    }
}
