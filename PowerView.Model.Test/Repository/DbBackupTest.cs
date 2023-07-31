using System;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using PowerView.Model.Repository;
using Microsoft.Extensions.Logging.Abstractions;

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
            File.Delete(DbName + "-journal");
        }

        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            var logger = new NullLogger<DbBackup>();
            var dbOptions = new DatabaseOptions();
            var bckOptions = new DatabaseBackupOptions();

            // Act & Assert
            Assert.That(() => new DbBackup(null, dbOptions, bckOptions), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new DbBackup(logger, null, bckOptions), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new DbBackup(logger, dbOptions, null), Throws.TypeOf<ArgumentNullException>());
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
        public void IgnoreCustomFile()
        {
            // Arrange
            CreateFile("Custom-file");
            var target = CreateTarget();

            // Act
            target.BackupDatabaseAsNeeded(false);

            // Assert
            Assert.That(new DirectoryInfo(backupPath).GetFiles().Length, Is.EqualTo(3));
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
            File.WriteAllText(DbName + "-journal", string.Empty);
            return new DbBackup(new NullLogger<DbBackup>(), new DatabaseOptions { Name = DbName }, new DatabaseBackupOptions { MinimumInterval = TimeSpan.FromDays(14), MaximumCount = 4 });
        }

        private void CreateBackup(int daysAgo)
        {
            var dateTime = DateTime.Now - TimeSpan.FromDays(daysAgo);
            var backupTime = dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            CreateFile(backupTime + "_" + Path.GetFileName(DbName));
            CreateFile(backupTime + "_" + Path.GetFileName(DbName) + "-journal");
        }

        private void CreateFile(string fileName)
        {
            File.WriteAllText(Path.Combine(backupPath, fileName), string.Empty);
        }
    }
}
