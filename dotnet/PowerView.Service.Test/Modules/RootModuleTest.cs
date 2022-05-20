/*
using System;
using PowerView.Service.Modules;
using Nancy.Testing;
using NUnit.Framework;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class RootModuleTest
  {
    private Browser browser;

    [SetUp]
    public void SetUp()
    {
      browser = new Browser(cfg => cfg.Module<RootModule>());
    }

    [Test]
    public void GetRootRouteRedirect()
    {
      // Arrange

      // Act
      var result = browser.Get("", with => with.HttpRequest());

      // Assert
      result.ShouldHaveRedirectedTo("web/index.html", StringComparison.InvariantCultureIgnoreCase);
    }

    [Test]
    public void GetIndexHtmlRouteRedirect()
    {
      // Arrange

      // Act
      var result = browser.Get("index.html", with => with.HttpRequest());

      // Assert
      result.ShouldHaveRedirectedTo("web/index.html", StringComparison.InvariantCultureIgnoreCase);
    }

  }
}
*/