using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class ProfileGraphTest
  {
    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      const string period = "day";
      const string page = "thePage";
      const string title = "theTitle";
      const string interval = "5-minutes";
      const long rank = 1;
      var serieNames = new[] { new SerieName("label", ObisCode.ElectrActiveEnergyA14Period), new SerieName("label2", ObisCode.ElectrActualPowerP14) };

      // Act
      var target = new ProfileGraph(period, page, title, interval, rank, serieNames);

      // Assert
      Assert.That(target.Period, Is.EqualTo(period));
      Assert.That(target.Page, Is.EqualTo(page));
      Assert.That(target.Title, Is.EqualTo(title));
      Assert.That(target.Interval, Is.EqualTo(interval));
      Assert.That(target.Rank, Is.EqualTo(rank));
      Assert.That(target.SerieNames, Is.EqualTo(serieNames));
    }

    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      const string period = "day";
      const string page = "thePage";
      const string title = "theTitle";
      const string interval = "5-minutes";
      const long rank = 1;
      var sn = new SerieName("label", ObisCode.ElectrActiveEnergyA14Period);
      var serieNames = new[] { sn };

      // Act & Assert
      Assert.That(() => new ProfileGraph(null, page, title, interval, rank, serieNames), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new ProfileGraph("", page, title, interval, rank, serieNames), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new ProfileGraph("something", page, title, interval, rank, serieNames), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new ProfileGraph(period, null, title, interval, rank, serieNames), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new ProfileGraph(period, page, null, interval, rank, serieNames), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new ProfileGraph(period, page, "", interval, rank, serieNames), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new ProfileGraph(period, page, title, null, rank, serieNames), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new ProfileGraph(period, page, title, "", rank, serieNames), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new ProfileGraph(period, page, title, interval, rank, null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new ProfileGraph(period, page, title, interval, rank, new SerieName[] { }), Throws.TypeOf<ArgumentException>());
      Assert.That(() => new ProfileGraph(period, page, title, interval, rank, new SerieName[] { null }), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new ProfileGraph(period, page, title, interval, rank, new SerieName[] { sn, sn }), Throws.TypeOf<ArgumentException>());
    }

  }
}

