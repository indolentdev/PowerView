using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PowerView.Model.Repository
{
  internal class DbCheck : RepositoryBase, IDbCheck
  {
    private readonly int integrityCheckCommandTimeout;

    public DbCheck(IDbContext dbContext, int integrityCheckCommandTimeout)
      : base(dbContext)
    {
      this.integrityCheckCommandTimeout = integrityCheckCommandTimeout;
    }

    public void CheckDatabase()
    {
      IList<dynamic> integrityCheckResult;
      try
      {
        integrityCheckResult = DbContext.QueryNoTransaction<dynamic>("PRAGMA integrity_check;", commandTimeout: integrityCheckCommandTimeout).ToList();
      }
      catch (DataStoreCorruptException e)
      {
        throw new DataStoreCorruptException("Database integrity corrupted. Restore a previous backup.", e);
      }

      if (integrityCheckResult.Count != 1 || integrityCheckResult[0].integrity_check != "ok")
      {
        throw new DataStoreCorruptException("Database integrity corrupted. Restore a previous backup. Details:" + 
          string.Join("  -  ", integrityCheckResult));
      }
    }

  }
}
