using System;

namespace PowerView.Model.Repository
{
    internal class RepositoryBase
    {
        //    private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal RepositoryBase(IDbContext dbContext)
        {
            DbContext = ValidateDbContext(dbContext);
        }

        internal DbContext DbContext { get; private set; }

        private static DbContext ValidateDbContext(IDbContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(dbContext);
            var concreteDbContext = dbContext as DbContext;
            if (concreteDbContext == null) throw new ArgumentOutOfRangeException(nameof(dbContext), "Must be of type DbContext");
            return concreteDbContext;
        }

    }
}