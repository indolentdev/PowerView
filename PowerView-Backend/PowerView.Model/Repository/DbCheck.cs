using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PowerView.Model.Repository
{
    internal class DbCheck : RepositoryBase, IDbCheck
    {
        private readonly ILogger logger;

        private readonly IOptions<DatabaseCheckOptions> options;

        public DbCheck(ILogger<DbCheck> logger, IDbContext dbContext, IOptions<DatabaseCheckOptions> options)
          : base(dbContext)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void CheckDatabase()
        {
            var commandTimeout = options.Value.IntegrityCheckCommandTimeout;

            logger.LogInformation("Performing database integrity check.");

            IList<dynamic> integrityCheckResult;
            try
            {
                integrityCheckResult = DbContext.QueryNoTransaction<dynamic>("PRAGMA integrity_check;", commandTimeout: commandTimeout).ToList();
            }
            catch (DataStoreCorruptException e)
            {
                throw new DataStoreCorruptException("Database integrity corrupted. Restore a previous backup.", e);
            }
            catch (DataStoreException e)
            {
                throw new DataStoreException($"Database integrity check failed. Command timeout:{commandTimeout}.", e);
            }

            if (integrityCheckResult.Count != 1 || integrityCheckResult[0].integrity_check != "ok")
            {
                throw new DataStoreCorruptException("Database integrity corrupted. Restore a previous backup. Details:" +
                  string.Join("  -  ", integrityCheckResult));
            }

            logger.LogInformation($"Database integrity check completed.");
        }

    }
}
