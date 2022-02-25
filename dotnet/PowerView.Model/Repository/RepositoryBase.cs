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

    internal int GetPageCount(int take)
    {
      if (take < 1) return 50;
      if (take > 100) return 100;

      return take;
    }

    private static DbContext ValidateDbContext(IDbContext dbContext)
    {
      if ( dbContext == null ) throw new ArgumentNullException("dbContext");
      var concreteDbContext = dbContext as DbContext;
      if ( concreteDbContext == null ) throw new ArgumentOutOfRangeException("dbContext", "Must be of type DbContext");
      return concreteDbContext;
    }

  }
}