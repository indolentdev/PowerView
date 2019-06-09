﻿using System;

namespace PowerView.Model
{
  public class SerieName : ISerieName, IEquatable<SerieName>
  {
    public SerieName(string label, ObisCode obisCode)
    {
      if (string.IsNullOrEmpty(label)) throw new ArgumentNullException("label");

      Label = label;
      ObisCode = obisCode;
    }

    public string Label { get; private set; }
    public ObisCode ObisCode { get; private set; }

    public override bool Equals(object obj)
    {
      if (!(obj is SerieName)) return false;
      return Equals((SerieName)obj);
    }

    public bool Equals(SerieName other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return string.Equals(Label, other.Label, StringComparison.InvariantCulture) &&
                   ObisCode.Equals(other.ObisCode);
    }

    public bool Equals(ISerieName obj)
    {
      return Equals((object)obj);
    }

    public override int GetHashCode()
    {
      unchecked  
      {
        var hashCode = Label.GetHashCode();
        hashCode = (hashCode * 397) ^ ObisCode.GetHashCode();
        return hashCode;
      } 
    }

  }
}

