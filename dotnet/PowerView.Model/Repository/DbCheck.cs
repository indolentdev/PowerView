using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace PowerView.Model.Repository
{
    internal class DbCheck : RepositoryBase, IDbCheck
    {
        private readonly IOptions<DatabaseCheckOptions> options;

        public DbCheck(IDbContext dbContext, IOptions<DatabaseCheckOptions> options)
          : base(dbContext)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void CheckDatabase()
        {
            var commandTimeout = options.Value.IntegrityCheckCommandTimeout;

            IList<dynamic> integrityCheckResult;
            try
            {
                integrityCheckResult = DbContext.QueryNoTransaction<dynamic>("PRAGMA integrity_check;", commandTimeout: commandTimeout).ToList();
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
