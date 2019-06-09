using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

namespace PowerView.Model.Repository
{
  internal class DbCheck : RepositoryBase, IDbCheck
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
        integrityCheckResult = DbContext.QueryNoTransaction<dynamic>("integrity check", "PRAGMA integrity_check;", commandTimeout: integrityCheckCommandTimeout).ToList();
      }
      catch (DataStoreCorruptException e)
      {
        throw new DataStoreCorruptException("Database integrity corrupted. Restore a previous backup.", e);
      }

      if (integrityCheckResult.Count != 1 || integrityCheckResult[0].integrity_check != "ok")
      {
        log.WarnFormat("Database integrity corrupted. Issues:" + string.Join(Environment.NewLine, integrityCheckResult));
        throw new DataStoreCorruptException("Database integrity corrupted. Restore a previous backup.");
      }
    }

  }
}
