
namespace PowerView.Model.Repository
{
  public interface IDbBackup
  {
    void BackupDatabaseAsNeeded(bool force);
  }
}

