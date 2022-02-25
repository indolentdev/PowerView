using System.Collections.Generic;
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

    [Test]
    [TestCase("SN", "SN", new[] { "SN" })]
    [TestCase("sn", "SN", new[] { "sn" })]
    [TestCase(null, null, new[] { (string)null })]
    [TestCase("SN", "XX", new[] { "SN", "XX" })]
    [TestCase(null, "SN", new[] { null, "SN" })]
    [TestCase("SN", null, new[] { "SN", null })]
    public void DistinctDeviceIdsSingleArray(string d1, string d2, string[] expected)
    {
      // Arrange
      IEnumerable<string> deviceIds = new[] { d1, d2 };

      // Act
      var distinctDeviceIds = DeviceId.DistinctDeviceIds(deviceIds);

      // Assert
      Assert.That(distinctDeviceIds, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("SN", "SN", new[] { "SN" })]
    [TestCase("sn", "SN", new[] { "sn" })]
    [TestCase(null, null, new[] { (string)null })]
    [TestCase("SN", "XX", new[] { "SN", "XX" })]
    [TestCase(null, "SN", new[] { null, "SN" })]
    [TestCase("SN", null, new[] { "SN", null })]
    public void DistinctDeviceIdsMultiArrays(string d1, string d2, string[] expected)
    {
      // Arrange
      IEnumerable<string> deviceIds1 = new[] { d1 };
      IEnumerable<string> deviceIds2 = new[] { d2 };

      // Act
      var distinctDeviceIds = DeviceId.DistinctDeviceIds(deviceIds1, deviceIds2);

      // Assert
      Assert.That(distinctDeviceIds, Is.EqualTo(expected));
    }


  }
}
