using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;
using Microsoft.Extensions.Logging.Abstractions;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class DbMigrateTest : DbTestFixtureWithSchema
  {
    // Test obsolete.. Kept here for reference..
/*    
    private void MigrateVersion6PopulateSerialNumber()
    {
      // Arrange
      var lr1a = new Db.LiveReading {Label="Label1"};
      var lr1b = new Db.LiveReading {Label="Label1"};
      var lr2a = new Db.LiveReading {Label="Label2"};
      var lr2b = new Db.LiveReading {Label="Label2"};
      var lr3a = new Db.LiveReading {Label="Label3"};
      var lr3b = new Db.LiveReading {Label="Label3"};
      var lr1z = new Db.LiveReading {Label="Label1", DeviceId="111"};
      var lr2z = new Db.LiveReading {Label="Label2", DeviceId="222"};
      var lr3z = new Db.LiveReading {Label="Label3"};
      SetTimestamp(new[] { lr1a, lr1b, lr2a, lr2b, lr3a, lr3b, lr1z, lr2z, lr3z });
      DbContext.InsertReadings(lr1a, lr1b, lr2a, lr2b, lr3a, lr3b, lr1z, lr2z, lr3z);
      var target = CreateTarget();

      // Act
      target.Migrate();

      // Assert
      AssertLiveReadings(3, "Label1", "111");
      AssertLiveReadings(3, "Label2", "222");
    }
*/      
    private static void SetTimestamp(IEnumerable<Db.LiveReading> readings)
    {
      var dateTime = DateTime.Now;
      foreach (var reading in readings)
      {
        reading.Timestamp = dateTime;
      }
    }

    private void AssertLiveReadings(int expectedCount, string label, string serialNumber)
    {
      var result = DbContext.QueryTransaction<Db.LiveReading>("SELECT * FROM LiveReading WHERE Label=@label AND SerialNumber=@serialNumber", new { label, serialNumber });
      Assert.That(result.Count(), Is.EqualTo(expectedCount));
    }

    private DbMigrate CreateTarget()
    {
      return new DbMigrate(new NullLogger<DbMigrate>(), DbContext);
    }

  }
}
