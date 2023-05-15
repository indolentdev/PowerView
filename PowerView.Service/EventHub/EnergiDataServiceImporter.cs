using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service;
using PowerView.Service.EnergiDataService;

namespace PowerView.Service.EventHub
{
    public class EnergiDataServiceImporter : IEnergiDataServiceImporter
    {
        private readonly ILogger logger;
        private readonly ISettingRepository settingRepository;
        private readonly ILocationContext locationContext;
        private readonly IEnergiDataServiceClient energiDataServiceClient;
        private readonly IReadingAccepter readingAccepter;

        public EnergiDataServiceImporter(ILogger<EnergiDataServiceImporter> logger, ISettingRepository settingRepository, ILocationContext locationContext, IEnergiDataServiceClient energiDataServiceClient, IReadingAccepter readingAccepter)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.settingRepository = settingRepository ?? throw new ArgumentNullException(nameof(settingRepository));
            this.locationContext = locationContext ?? throw new ArgumentNullException(nameof(locationContext));
            this.energiDataServiceClient = energiDataServiceClient ?? throw new ArgumentNullException(nameof(energiDataServiceClient));
            this.readingAccepter = readingAccepter ?? throw new ArgumentNullException(nameof(readingAccepter));
        }

        public async Task Import()
        {
            EnergiDataServiceImportConfig config;
            try
            {
                config = settingRepository.GetEnergiDataServiceImportConfig();
            }
            catch (DomainConstraintException e)
            {
                logger.LogDebug(e, $"Skipping EnergiDataService import. Configuration absent or incomplete.");
                return;
            }

            if (!config.ImportEnabled)
            {
                logger.LogDebug($"EnergiDataService import disabled");
                return;
            }

            var start = settingRepository.GetEnergiDataServiceImportPosition();
            if (start == null) 
            {
                logger.LogDebug($"EnergiDataService import starting from today");
                start = locationContext.ConvertTimeToUtc(DateTime.Today);
            }

            IList<KwhAmount> kwhAmounts;
            try
            {
                kwhAmounts = await energiDataServiceClient.GetElectricityAmounts(start.Value, config.TimeSpan, config.PriceArea);
            }
            catch (EnergiDataServiceClientException e)
            {
                logger.LogInformation(e, $"Could not import income/expense values. Web request for EnergiDataService failed. Import will be retried later.");
                return;
            }

            logger.LogDebug($"Fetched {kwhAmounts.Count} values from Energi Data Service");

            if (kwhAmounts.Count == 0) return;

            readingAccepter.Accept(ToReadings(kwhAmounts, config.Label, "EnergiDataService", config.Currency));
            settingRepository.UpsertEnergiDataServiceImportPosition(kwhAmounts.Select(x => x.Start).Max());
        }

        private IList<Reading> ToReadings(IList<KwhAmount> kwhAmounts, string label, string deviceId, Unit currency)
        {
            return kwhAmounts
              .Select(x => new Reading(label, deviceId, x.Start, new[] { GetRegisterValue(x, currency) }))
              .ToList();
        }

        private RegisterValue GetRegisterValue(KwhAmount kwhAmount, Unit currency)
        {
            double amount;
            switch (currency)
            {
                case Unit.Dkk:
                    amount = kwhAmount.AmountDkk;
                    break;
                case Unit.Eur:
                    amount = kwhAmount.AmountEur;
                    break;
                default:
                    throw new NotSupportedException($"Currency unit not supported:{currency}");
            }

            var (value, scale) = LossyToRegisterValue(amount);

            return new RegisterValue(ObisCode.ElectrActiveEnergyKwhIncomeExpense, value, scale, currency, RegisterValueTag.Import);
        }

        internal static (int Value, short Scale) LossyToRegisterValue(double amount)
        {
            var amountIntegral = (int)Math.Truncate(amount);
            var amountDecimal = amount - amountIntegral;    
            short iterations = 0;
            while (amountDecimal != 0 && iterations < 7)
            {
                iterations++;
                amountDecimal = amountDecimal * 10;
                var decimalDigit = Math.Truncate(amountDecimal);
                amountIntegral = amountIntegral * 10 + (int)decimalDigit;

                amountDecimal = amountDecimal - decimalDigit;
            }
            return (amountIntegral, (short)(iterations * -1));
        }
    }
}

