using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Channels;
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
        private readonly IImportRepository importRepository;
        private readonly IEnergiDataServiceClient energiDataServiceClient;
        private readonly IReadingAccepter readingAccepter;

        public EnergiDataServiceImporter(ILogger<EnergiDataServiceImporter> logger, IImportRepository importRepository, IEnergiDataServiceClient energiDataServiceClient, IReadingAccepter readingAccepter)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.importRepository = importRepository ?? throw new ArgumentNullException(nameof(importRepository));
            this.energiDataServiceClient = energiDataServiceClient ?? throw new ArgumentNullException(nameof(energiDataServiceClient));
            this.readingAccepter = readingAccepter ?? throw new ArgumentNullException(nameof(readingAccepter));
        }

        public async Task Import(DateTime timestamp)
        {
            ArgCheck.ThrowIfNotUtc(timestamp);

            var enabledImportGroups = GetImportGroups(timestamp, importRepository.GetImports());

            if (enabledImportGroups.Count == 0) return;

            logger.LogTrace("EnergiDataService import. {Count} enabled imports.", enabledImportGroups.Sum(x => x.Imports.Count));

            foreach (var importGroup in enabledImportGroups)
            {
                IList<KwhAmount> kwhAmounts;
                try
                {
                    kwhAmounts = await energiDataServiceClient.GetElectricityAmounts(importGroup.PositionTimestamp, importGroup.TimeSpan, importGroup.Channel);
                }
                catch (EnergiDataServiceClientException e)
                {
                    var hint = " Enable Debug log level for more information.";
                    Exception logException = null;
                    if (logger.IsEnabled(LogLevel.Debug) || logger.IsEnabled(LogLevel.Trace))
                    {
                        logException = e;
                        hint = string.Empty;
                    }
                    logger.LogInformation(logException, "Could not import income/expense values. Web request for EnergiDataService failed. Import will be retried later.{Hint}", hint);
                    continue;
                }

                if (kwhAmounts.Count == 0) continue;

                foreach (var import in importGroup.Imports)
                {
                    List<Reading> amountReadings;
                    try
                    {
                        amountReadings = ToReadings(kwhAmounts, import.Label, "EnergiDataService", import.Currency);
                    }
                    catch (ModelException e)
                    {
                        logger.LogInformation(e, "Unable to add imported readings for label:{Label}. Conversion to reading failed", import.Label);
                        return;
                    }

                    try
                    {
                        logger.LogDebug("Importing {Count} values from Energi Data Service for label {Label}", amountReadings.Sum(x => x.GetRegisterValues().Count), import.Label);
                        readingAccepter.Accept(amountReadings);
                    }
                    catch (DataStoreBusyException e)
                    {
                        Exception ex = null;
                        if (logger.IsEnabled(LogLevel.Debug))
                        {
                            ex = e;
                        }
                        logger.LogInformation(ex, "Unable to add imported readings for label:{Label}. Data store busy. Going to retry on next trigger.", import.Label);
                        return;
                    }
                    var lastKwhAmount = kwhAmounts.OrderBy(x => x.Start).Last();
                    importRepository.SetCurrentTimestamp(import.Label, lastKwhAmount.Start + lastKwhAmount.Duration);
                }
            }
        }

        private static List<ImportGroup> GetImportGroups(DateTime timestamp, ICollection<Import> imports)
        {
            var groups = imports
              .Where(x => x.Enabled)
              .Select(x => new { Import = x, PositionTimestamp = x.CurrentTimestamp != null ? x.CurrentTimestamp.Value : x.FromTimestamp })
              .GroupBy(x => new { x.Import.Channel, x.PositionTimestamp });

            var importGroups = new List<ImportGroup>();
            foreach (var group in groups)
            {
                var channel = group.Key.Channel;
                var positionTimestamp = group.Key.PositionTimestamp;
                var timeSpan = positionTimestamp < (timestamp - TimeSpan.FromDays(3)) ? TimeSpan.FromDays(3) : TimeSpan.FromHours(6);
                var importGroup = new ImportGroup
                {
                    Channel = channel,
                    PositionTimestamp = positionTimestamp,
                    TimeSpan = timeSpan,
                    Imports = group.Select(x => x.Import).ToList()
                };
                importGroups.Add(importGroup);
            }
            return importGroups;
        }

        private static List<Reading> ToReadings(IList<KwhAmount> kwhAmounts, string label, string deviceId, Unit currency)
        {
            return kwhAmounts
              .Select(x => new Reading(label, deviceId, x.Start, new[] { GetRegisterValue(x, currency) }))
              .ToList();
        }

        private static RegisterValue GetRegisterValue(KwhAmount kwhAmount, Unit currency)
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
            var obisCode = GetObisCode(kwhAmount);

            return new RegisterValue(obisCode, value, scale, currency, RegisterValueTag.Import);
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

        internal static ObisCode GetObisCode(KwhAmount kwhAmount)
        {
            switch (kwhAmount.Duration)
            {
                case var d when d == TimeSpan.FromHours(1):
                    return ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVatH;

                case var d when d == TimeSpan.FromMinutes(15):
                    return ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVatQ;

                default:
                    throw new  ModelException($"Cannot convert KwhAmount duration to obis code. Duration was:{kwhAmount.Duration}");
            }
        }

        private class ImportGroup
        {
            public string Channel { get; set; }
            public DateTime PositionTimestamp { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public List<Import> Imports { get; set; }
        }
    }
}

