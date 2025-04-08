using PowerView.Model.Repository;

namespace PowerView
{
    public class DbSetup : IDbSetup
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;

        public DbSetup(ILogger<DbSetup> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void SetupDatabase()
        {
            DapperConfig.Configure();

            var dbUpgradeNeeded = false;
            using (var scope = serviceProvider.CreateScope())
            {
                var envRepository = scope.ServiceProvider.GetRequiredService<IEnvironmentRepository>();
                var sqliteVersion = envRepository.GetSqliteVersion();
                logger.LogInformation("SQLite version:{Version}", sqliteVersion);

                var dbCheck = scope.ServiceProvider.GetRequiredService<IDbCheck>();
                dbCheck.CheckDatabase();

                var dbUpgrade = scope.ServiceProvider.GetRequiredService<IDbUpgrade>();
                dbUpgradeNeeded = dbUpgrade.IsNeeded();
            }

            var dbBackup = serviceProvider.GetRequiredService<IDbBackup>();
            dbBackup.BackupDatabaseAsNeeded(dbUpgradeNeeded);

            using (var scope = serviceProvider.CreateScope())
            {
                var dbUpgrade = scope.ServiceProvider.GetRequiredService<IDbUpgrade>();
                dbUpgrade.ApplyUpdates();

                var dbMigrate = scope.ServiceProvider.GetRequiredService<IDbMigrate>();
                dbMigrate.Migrate();
            }
        }

    }
}
