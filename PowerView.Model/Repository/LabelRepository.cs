
namespace PowerView.Model.Repository
{
    internal class LabelRepository : RepositoryBase, ILabelRepository
    {
        public LabelRepository(IDbContext dbContext)
          : base(dbContext)
        {
        }

        public IList<string> GetLabelsByTimestamp()
        {
            string sqlQuery = @"
SELECT LabelName
FROM Label
ORDER BY Timestamp DESC
;";

            return DbContext.QueryTransaction<string>(sqlQuery);
        }

    }

}
