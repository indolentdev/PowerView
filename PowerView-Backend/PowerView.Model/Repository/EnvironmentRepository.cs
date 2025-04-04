using System.Linq;

namespace PowerView.Model.Repository
{
    internal class EnvironmentRepository : RepositoryBase, IEnvironmentRepository
    {
        //    private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EnvironmentRepository(IDbContext dbContext)
          : base(dbContext)
        {
        }

        public string GetSqliteVersion()
        {
            var sqliteVersion = DbContext.QueryNoTransaction<string>("SELECT sqlite_version();").FirstOrDefault();
            return sqliteVersion;
        }

    }
}
