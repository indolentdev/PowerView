/*
using System;
using System.Globalization;
using System.Linq;
using PowerView.Service.Modules;
using NUnit.Framework;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class PeriodTest
  {
    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      var from = DateTime.Now;
      var to = DateTime.UtcNow.AddMinutes(63);

      // Act
      var target = new ExportModule.Period(from, to);

      // Assert
      Assert.That(target.From, Is.EqualTo(from));
      Assert.That(target.To, Is.EqualTo(to));
    }

    [Test]
    public void Equals()
    {
      // Arrange
      var from = DateTime.Now;
      var to = DateTime.UtcNow.AddMinutes(63);
      var t1 = new ExportModule.Period(from, to);
      var t2 = new ExportModule.Period(from, to);
      var t3 = new ExportModule.Period(from.AddMilliseconds(1), to);
      var t4 = new ExportModule.Period(from, to.AddMilliseconds(1));

      // Act & Assert
      Assert.That(t1.Equals(t2), Is.True);
      Assert.That(t1.Equals((object)t2), Is.True);
      Assert.That(t1.Equals(t3), Is.False);
      Assert.That(t1.Equals(t4), Is.False);
    }

    [Test]
    public void GetHashcode()
    {
      // Arrange
      var from = DateTime.Now;
      var to = DateTime.UtcNow.AddMinutes(63);
      var t1 = new ExportModule.Period(from, to);
      var t2 = new ExportModule.Period(from, to);
      var t3 = new ExportModule.Period(from.AddMilliseconds(1), to);
      var t4 = new ExportModule.Period(from, to.AddMilliseconds(1));

      // Act & Assert
      Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t3.GetHashCode()));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t4.GetHashCode()));
    }

    [Test]
    [TestCase("20210402T123345", "20210411T123345", "20210402T123345", "20210411T123345", 0)]
    [TestCase("20210402T123345", "20210411T123345", "20210403T123345", "20210411T123345", -1)]
    [TestCase("20210402T123345", "20210411T123345", "20210401T123345", "20210411T123345", 1)]
    [TestCase("20210402T123345", "20210411T123345", "20210402T123345", "20210413T123345", -1)]
    [TestCase("20210402T123345", "20210411T123345", "20210402T123345", "20210410T123345", 1)]
    public void CompareTo(string t1FromString, string t1ToString, string t2FromString, string t2ToString, int value)
    {
      // Arrange
      const string format = "yyyyMMddThhmmss";
      var t1From = DateTime.ParseExact(t1FromString, format, CultureInfo.InvariantCulture);
      var t1To = DateTime.ParseExact(t1ToString, format, CultureInfo.InvariantCulture);
      var t2From = DateTime.ParseExact(t2FromString, format, CultureInfo.InvariantCulture);
      var t2To = DateTime.ParseExact(t2ToString, format, CultureInfo.InvariantCulture);
      var t1 = new ExportModule.Period(t1From, t1To);
      var t2 = new ExportModule.Period(t2From, t2To);

      // Act
      var compareValue = t1.CompareTo(t2);

      // Assert
      Assert.That(compareValue, Is.EqualTo(value));
    }

    [Test]
    public void Comparable()
    {
      // Arrange
      var from = DateTime.Now;
      var to = DateTime.UtcNow.AddMinutes(63);
      var t1 = new ExportModule.Period(from, to);
      var t2 = new ExportModule.Period(from.AddMilliseconds(1), to.AddMilliseconds(1));
      var list = new[] { t2, t1 };

      // Act
      var orderedList = list.OrderBy(x => x);

      // Assert
      Assert.That(orderedList, Is.EqualTo(new[] { t1, t2 }));
    }

  }
}
*/