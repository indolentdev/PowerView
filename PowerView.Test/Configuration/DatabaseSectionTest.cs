using NUnit.Framework;
using PowerView.Configuration;

namespace PowerView.Test.Configuration
{
  [TestFixture]
  public class DatabaseSectionTest
  {
    private DatabaseSection target;

    [SetUp]
    public void SetUp()
    {
      target = new DatabaseSection { 
        Name = "TheDbName".ToStringElement(),
        Backup = new BackupElement { MinimumIntervalDays = 14.ToIntElement(), MaximumCount = 2.ToIntElement() },
        TimeZone = "Europe/Copenhagen".ToStringElement(),
        CultureInfo = "da-DK".ToStringElement()
      };
    }

    [Test]
    public void Validate()
    {
      // Arrange

      // Act
      target.Validate();

      // Assert
      Assert.That(target.HasBackupElement, Is.True);
    }

    [Test]
    public void ValidateBackupNull()
    {
      // Arrange
      target.Backup = null;

      // Act
      target.Validate();

      // Assert
      Assert.That(target.HasBackupElement, Is.False);
    }
  }
}

