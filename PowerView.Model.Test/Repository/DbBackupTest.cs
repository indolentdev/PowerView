using System;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class DbBackupTest
  {
    private const string DbName = "DbBackupTestDbName";

    private string backupPath;

    [SetUp]
    public void SetUp()
    {
      const string BackupPath = "DbBackup";
      backupPath = Path.Combine(Path.GetDirectoryName(DbName), BackupPath);
      Directory.CreateDirectory(backupPath);
    }

    [TearDown]
    public void TearDown()
    {
      Directory.Delete(backupPath, true);
      File.Delete(DbName);
      File.Delete(DbName+"-journal");
    }

    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var minTimeSpan = TimeSpan.FromDays(14);
      const int maxBackupCount = 10;

      // Act & Assert
      Assert.That(() => new DbBackup(null, minTimeSpan, maxBackupCount), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new DbBackup(string.Empty, minTimeSpan, maxBackupCount), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new DbBackup(DbName, minTimeSpan-TimeSpan.FromDays(1), maxBackupCount), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new DbBackup(DbName, minTimeSpan, 0), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void InitialRun()
    {
      // Arrange
      Directory.Delete(backupPath, true);
      var target = CreateTarget();

      // Act
      target.BackupDatabaseAsNeeded(false);

      // Assert
      Assert.That(Directory.Exists(backupPath), Is.True);
      Assert.That(new DirectoryInfo(backupPath).GetFiles().Length, Is.EqualTo(2));
      AssertDbBackupExists();
    }

    [Test]
    public void LessThanMinimumTimeSpan()
    {
      // Arrange
      const int daysAgo = 13;
      CreateBackup(daysAgo);
      var target = CreateTarget();

      // Act
      target.BackupDatabaseAsNeeded(false);

      // Assert
      Assert.That(new DirectoryInfo(backupPath).GetFiles().Length, Is.EqualTo(2));
      AssertDbBackupExists(daysAgo);
    }

    [Test]
    public void Force()
    {
      // Arrange
      const int daysAgo = 13;
      CreateBackup(daysAgo);
      var target = CreateTarget();

      // Act
      target.BackupDatabaseAsNeeded(true);

      // Assert
      Assert.That(new DirectoryInfo(backupPath).GetFiles().Length, Is.EqualTo(4));
      AssertDbBackupExists(daysAgo);
      AssertDbBackupExists();
    }

    [Test]
    public void GreaterThanMinimumTimeSpan()
    {
      // Arrange
      const int daysAgo = 15;
      CreateBackup(daysAgo);
      var target = CreateTarget();

      // Act
      target.BackupDatabaseAsNeeded(false);

      // Assert
      Assert.That(new DirectoryInfo(backupPath).GetFiles().Length, Is.EqualTo(4));
      AssertDbBackupExists(daysAgo);
      AssertDbBackupExists();
    }

    [Test]
    public void MaximumBackupCount()
    {
      // Arrange
      const int daysAgo1 = 15;
      CreateBackup(daysAgo1);
      const int daysAgo2 = 30;
      CreateBackup(daysAgo2);
      const int daysAgo3 = 45;
      CreateBackup(daysAgo3);
      const int daysAgo4 = 60;
      CreateBackup(daysAgo4);
      var target = CreateTarget();

      // Act
      target.BackupDatabaseAsNeeded(false);

      // Assert
      Assert.That(new DirectoryInfo(backupPath).GetFiles().Length, Is.EqualTo(8));
      AssertDbBackupExists(daysAgo3);
      AssertDbBackupExists(daysAgo2);
      AssertDbBackupExists(daysAgo1);
      AssertDbBackupExists();
    }

    private void AssertDbBackupExists(int daysAgo = 0)
    {
      var dateTime = DateTime.Now - TimeSpan.FromDays(daysAgo);
      var backupTime = dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
      Assert.That(File.Exists(Path.Combine(backupPath, backupTime + "_" + Path.GetFileName(DbName))), Is.True);
      Assert.That(File.Exists(Path.Combine(backupPath, backupTime + "_" + Path.GetFileName(DbName) + "-journal")), Is.True);
    }

    private static DbBackup CreateTarget()
    {
      File.WriteAllText(DbName, string.Empty);
      File.WriteAllText(DbName+"-journal", string.Empty);
      return new DbBackup(DbName, TimeSpan.FromDays(14), 4);
    }

    private void CreateBackup(int daysAgo)
    {
      var dateTime = DateTime.Now - TimeSpan.FromDays(daysAgo);
      var backupTime = dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
      File.WriteAllText(Path.Combine(backupPath, backupTime + "_" + Path.GetFileName(DbName)), string.Empty);
      File.WriteAllText(Path.Combine(backupPath, backupTime + "_" + Path.GetFileName(DbName) + "-journal"), string.Empty);
    }
  }
}
