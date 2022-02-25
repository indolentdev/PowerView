using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class DisconnectRuleTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var name = new SeriesName("Lbl", ObisCode.ElectrActualPowerP14);
      var duration = TimeSpan.FromMinutes(30);
      var unit = Unit.Watt;

      // Act & Assert
      Assert.That(() => new DisconnectRule(null, name, duration, 1, 2, unit), Throws.ArgumentNullException);
      Assert.That(() => new DisconnectRule(name, null, duration, 1, 2, unit), Throws.ArgumentNullException);
      Assert.That(() => new DisconnectRule(name, name, TimeSpan.FromSeconds(14 * 60 + 59), 1, 2, unit), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new DisconnectRule(name, name, TimeSpan.FromSeconds(6 * 60 * 60 + 1), 1, 2, unit), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new DisconnectRule(name, name, duration, 0, 2, unit), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new DisconnectRule(name, name, duration, 1, 0, unit), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new DisconnectRule(name, name, duration, 2, 2, unit), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new DisconnectRule(name, name, duration, 1, 2, (Unit)233), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      var name = new SeriesName("Lbl1", ObisCode.ElectrActualPowerP14);
      var evaluationName = new SeriesName("Lbl2", ObisCode.ElectrActualPowerP23);
      var duration = TimeSpan.FromMinutes(30);
      var dtc = 10;
      var ctd = 5;
      var unit = Unit.WattHour;

      // Act
      var target = new DisconnectRule(name, evaluationName, duration, dtc, ctd, unit);

      // Assert
      Assert.That(target.Name, Is.SameAs(name));
      Assert.That(target.EvaluationName, Is.SameAs(evaluationName));
      Assert.That(target.Duration, Is.EqualTo(duration));
      Assert.That(target.DisconnectToConnectValue, Is.EqualTo(dtc));
      Assert.That(target.ConnectToDisconnectValue, Is.EqualTo(ctd));
      Assert.That(target.Unit, Is.EqualTo(unit));
    }

    [Test]
    public void EqualsAndHashCode()
    {
      // Arrange
      var t1 = new DisconnectRule(new SeriesName("lbl", ObisCode.ElectrActualPowerP14), new SeriesName("lbl2", ObisCode.ElectrActualPowerP23), TimeSpan.FromMinutes(30), 3, 1, Unit.Watt);
      var t2 = new DisconnectRule(new SeriesName("lbl", ObisCode.ElectrActualPowerP14), new SeriesName("lbl2", ObisCode.ElectrActualPowerP23), TimeSpan.FromMinutes(30), 3, 1, Unit.Watt);
      var t3 = new DisconnectRule(new SeriesName("FFF", ObisCode.ElectrActualPowerP14), new SeriesName("lbl2", ObisCode.ElectrActualPowerP23), TimeSpan.FromMinutes(30), 3, 1, Unit.Watt);
      var t4 = new DisconnectRule(new SeriesName("lbl", ObisCode.ElectrActualPowerP14), new SeriesName("FFFF", ObisCode.ElectrActualPowerP23), TimeSpan.FromMinutes(30), 3, 1, Unit.Watt);
      var t5 = new DisconnectRule(new SeriesName("lbl", ObisCode.ElectrActualPowerP14), new SeriesName("lbl2", ObisCode.ElectrActualPowerP23), TimeSpan.FromMinutes(99), 3, 1, Unit.Watt);
      var t6 = new DisconnectRule(new SeriesName("lbl", ObisCode.ElectrActualPowerP14), new SeriesName("lbl2", ObisCode.ElectrActualPowerP23), TimeSpan.FromMinutes(30), 4, 1, Unit.Watt);
      var t7 = new DisconnectRule(new SeriesName("lbl", ObisCode.ElectrActualPowerP14), new SeriesName("lbl2", ObisCode.ElectrActualPowerP23), TimeSpan.FromMinutes(30), 3, 2, Unit.Watt);
      var t8 = new DisconnectRule(new SeriesName("lbl", ObisCode.ElectrActualPowerP14), new SeriesName("lbl2", ObisCode.ElectrActualPowerP23), TimeSpan.FromMinutes(30), 3, 1, Unit.Joule);

      // Act & Assert
      Assert.That(t1, Is.EqualTo(t2));
      Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t3));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t3.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t4));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t4.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t5));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t5.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t6));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t6.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t7));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t7.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t7));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t7.GetHashCode()));

      Assert.That(t1.Equals((IDisconnectRule)t2), Is.True);
      Assert.That(t1.Equals((object)t2), Is.True);
      Assert.That(t1.Equals((IDisconnectRule)t3), Is.False);
      Assert.That(t1.Equals((object)t3), Is.False);
    }

  }
}
