using System;
using System.IO;
using NUnit.Framework;
using PowerView.Configuration.Migration;

namespace PowerView.Test.Configuration.Migration
{
  [TestFixture]
  public class ConfigMigraterTest
  {
    private const string Configuration = "Configuration";
    private const string Migration = "Migration";
    private const string origFileName = "test.config";

    private string fileName;

    [SetUp]
    public void SetUp()
    {
      var now = DateTime.Now;
      fileName = now.ToString("o") + "_" + origFileName;

      var path = GetPath();
      File.Copy(Path.Combine(path, origFileName), Path.Combine(path, fileName));
    }

    [TearDown]
    public void TearDown()
    {
      foreach (var file in new DirectoryInfo(GetPath()).GetFiles(fileName + "*"))
      {
        file.Delete();
      }
    }

    [Test]
    public void Migrate()
    {
      // Arrange
      var target = new ConfigMigrater();
      var configFile = GetFileName();

      // Act
      target.Migrate(configFile);

      // Assert
      var xml = File.ReadAllText(configFile).Replace(Environment.NewLine, string.Empty).Replace("\t", string.Empty);

      StringAssert.DoesNotContain("Register", xml); // Version 0.0.29 -> 0.0.30
    }

    private string GetFileName()
    {
      return Path.Combine(GetPath(), fileName);
    }

    private string GetPath()
    {
      var location = new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location);
      var path = Path.GetDirectoryName(location.AbsolutePath);
      path = Path.Combine(Path.Combine(path, Configuration), Migration);
      return path;
    }

  }
}
