using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Moq;
using PowerView.Service.Translations;
using PowerView.Model;

namespace PowerView.Service.Test.Translations
{
  [TestFixture]
  public class TranslationTest
  {
    [Test]
    public void GetSameResourceDifferentCultures()
    {
      // Arrange
      var target1 = CreateTarget("en-GB");
      var target2 = CreateTarget("da-DK");
      const ResId resId = ResId.MailMessageTest;

      // Act
      var res1 = target1.Get(resId);
      var res2 = target2.Get(resId);

      // Assert
      Assert.That(res1, Is.Not.Null);
      Assert.That(res2, Is.Not.Null);
      Assert.That(res1, Is.Not.EqualTo(res2));
    }

    [Test]
    public void GetWith1Arg()
    {
      // Arrange
      var target = CreateTarget("da-DK");
      const ResId resId = ResId.MailMessageTest;

      // Act
      var res = target.Get(resId, "arg1");

      // Assert
      Assert.That(res, Is.Not.Null);
    }

    [Test]
    public void GetWith2Args()
    {
      // Arrange
      var target = CreateTarget("da-DK");
      const ResId resId = ResId.MailMessageTest;

      // Act
      var res = target.Get(resId, "arg1", "arg2");

      // Assert
      Assert.That(res, Is.Not.Null);
    }

    [Test]
    public void GetWith3Args()
    {
      // Arrange
      var target = CreateTarget("da-DK");
      const ResId resId = ResId.MailMessageTest;

      // Act
      var res = target.Get(resId, "arg1", "arg2", "arg3");

      // Assert
      Assert.That(res, Is.Not.Null);
    }

    [Test]
    public void GetWith4Args()
    {
      // Arrange
      var target = CreateTarget("da-DK");
      const ResId resId = ResId.MailMessageTest;

      // Act
      var res = target.Get(resId, "arg1", "arg2", "arg3", "args4");

      // Assert
      Assert.That(res, Is.Not.Null);
    }

    [Test]
    public void GetAllResourcesAllCultures()
    {
      // Arrange
      var allCultures = new[] { "en-GB", "da-DK" };
      var allResIds = (ResId[])Enum.GetValues(typeof(ResId));

      foreach (var cultureInfoName in allCultures)
      {
        var target = CreateTarget(cultureInfoName);
        foreach (var resId in allResIds)
        {
          // Act
          var res = target.Get(resId);

          // Assert
          Assert.That(res, Is.Not.Null.Or.Empty, cultureInfoName + " : " + resId);
        }
      }
    }

    private Translation CreateTarget(string cultureInfoName)
    {
      var cultureInfo = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                                   .Single(ci => ci.Name == cultureInfoName);
      var lc = new Mock<ILocationContext>();
      lc.Setup(x => x.CultureInfo).Returns(cultureInfo);
      return new Translation(lc.Object);
    }

  }
}
