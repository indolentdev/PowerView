using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class DeviceIdTest
  {
    [Test]
    [TestCase("SN", "SN", true)]
    [TestCase("sn", "SN", true)]
    [TestCase(null, null, true)]
    [TestCase("SN", "XX", false)]
    [TestCase(null, "SN", false)]
    [TestCase("SN", null, false)]
    public void Equals(string d1, string d2, bool expected)
    {
      // Arrange

      // Act
      var equals = DeviceId.Equals(d1, d2);

      // Assert
      Assert.That(equals, Is.EqualTo(expected));
    }
  }
}
