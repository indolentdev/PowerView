using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerView.Model.Repository;

namespace PowerView.Service.EventHub
{
    internal class EnergiDataServiceImport : IEnergiDataServiceImport
    {
        private readonly ILogger logger;
        private readonly TimeSpan minimumDayInterval = TimeSpan.FromDays(1);

        private readonly IIntervalTrigger intervalTrigger;

        public EnergiDataServiceImport(ILogger<EnergiDataServiceImport> logger, IIntervalTrigger intervalTrigger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.intervalTrigger = intervalTrigger ?? throw new ArgumentNullException(nameof(intervalTrigger));

            this.intervalTrigger.Setup(new TimeSpan(0, 4, 0), TimeSpan.FromHours(1));
        }

        public async Task Import(IServiceScope serviceScope, DateTime dateTime)
        {
            if (serviceScope == null) throw new ArgumentNullException(nameof(serviceScope));

            if (!intervalTrigger.IsTriggerTime(dateTime))
            {
                return;
            }

            var importer = serviceScope.ServiceProvider.GetRequiredService<IEnergiDataServiceImporter>();
            logger.LogDebug("Performing import from energi data service");
            await importer.Import(dateTime);
            intervalTrigger.Advance(dateTime);
        }

    }
}
