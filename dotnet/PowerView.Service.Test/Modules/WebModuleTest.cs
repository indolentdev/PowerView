/*
using System;
using System.IO;
using PowerView.Service.Modules;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class WebModuleTest
  {
    private Browser browser;

    private const string WebRoute = "/web/";
    private const string IndexHtmlContent = "The web SPA index content";
    private const string WebDirectory = "PowerView-Web";
    private const string IndexHtml = "index.html";

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      var directory = GetDirectory();
      if (!Directory.Exists(Path.Combine(directory, WebDirectory)))
      {
        Directory.CreateDirectory(Path.Combine(directory, WebDirectory));
      }
      File.WriteAllText(Path.Combine(Path.Combine(directory, WebDirectory), IndexHtml), IndexHtmlContent);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
      var directory = GetDirectory();
      if (Directory.Exists(Path.Combine(directory, WebDirectory)))
      {
        Directory.Delete(Path.Combine(directory, WebDirectory), true);
      }
    }

    [SetUp]
    public void SetUp()
    {
      browser = new Browser(cfg => cfg.Module<WebModule>());
    }

    [Test]
    [TestCase("level1")]
    [TestCase("level1/level2")]
    [TestCase("level1/level2/level3")]
    public void GetWebSublevel1(string level)
    {
      // Arrange

      // Act
      var response = browser.Get(WebRoute + level, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      Assert.That(response.Body.AsString(), Is.EqualTo(IndexHtmlContent));
    }

    private string GetDirectory()
    {
      var codeBase = new Uri(this.GetType().Assembly.CodeBase);
      var asmDirectory = Path.GetDirectoryName(codeBase.AbsolutePath);
      return asmDirectory;
    }
  }
}
*/