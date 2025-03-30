
namespace PowerView.Model.Repository
{
    public interface IDbContextFactory
    {
        IDbContext CreateContext();
    }
}

