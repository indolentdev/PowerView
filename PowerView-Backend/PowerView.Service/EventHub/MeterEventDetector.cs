using System;
using System.Collections.Generic;
using System.Linq;
using PowerView.Model;
using PowerView.Model.Repository;

namespace PowerView.Service.EventHub
{
    public class MeterEventDetector : IMeterEventDetector
    {
        private readonly IProfileRepository profileRepository;
        private readonly IMeterEventRepository meterEventRepository;
        private readonly ILocationContext locationContext;
        private readonly ILeakCharacteristicChecker leakCharacteristicChecker;

        public MeterEventDetector(IProfileRepository profileRepository, IMeterEventRepository meterEventRepository, ILocationContext locationContext, ILeakCharacteristicChecker leakCharacteristicChecker)
        {
            this.profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
            this.meterEventRepository = meterEventRepository ?? throw new ArgumentNullException(nameof(meterEventRepository));
            this.locationContext = locationContext ?? throw new ArgumentNullException(nameof(locationContext));
            this.leakCharacteristicChecker = leakCharacteristicChecker ?? throw new ArgumentNullException(nameof(leakCharacteristicChecker));
        }

        public void DetectMeterEvents(DateTime timestamp)
        {
            ArgCheck.ThrowIfNotUtc(timestamp);

            var dateLocal = locationContext.ConvertTimeFromUtc(timestamp);
            var localMidnightAsUtc = dateLocal.Date.ToUniversalTime();

            var end = localMidnightAsUtc.AddDays(1);
            var preStart = localMidnightAsUtc.Subtract(TimeSpan.FromMinutes(30));
            var labelSeriesSet = profileRepository.GetDayProfileSet(preStart, localMidnightAsUtc, end);

            var intervalGroup = new IntervalGroup(locationContext, localMidnightAsUtc, "5-minutes", labelSeriesSet, Array.Empty<CostBreakdownGeneratorSeries>());
            intervalGroup.Prepare();

            var meterEventCandidates = GetMeterEventCandidates(timestamp, localMidnightAsUtc, intervalGroup.NormalizedDurationLabelSeriesSet).ToArray();
            if (meterEventCandidates.Length == 0)
            {
                return;
            }

            var latestMeterEvents = meterEventRepository.GetLatestMeterEventsByLabel();

            var newMeterEvents = GetNewMeterEvents(meterEventCandidates, latestMeterEvents).ToArray();
            if (newMeterEvents.Length == 0)
            {
                return;
            }

            meterEventRepository.AddMeterEvents(newMeterEvents);
        }

        private IEnumerable<MeterEvent> GetMeterEventCandidates(DateTime timestamp, DateTime localMidnightAsUtc, LabelSeriesSet<NormalizedDurationRegisterValue> labelSeriesSet)
        {
            var coldWaterVolume1Delta = ObisCode.ColdWaterVolume1Delta;
            foreach (var labelSeries in labelSeriesSet)
            {
                if (!labelSeries.Contains(coldWaterVolume1Delta))
                {
                    continue;
                }

                var timestampNonUtc = locationContext.ConvertTimeFromUtc(localMidnightAsUtc);
                var start = locationContext.ConvertTimeToUtc(new DateTime(timestampNonUtc.Year, timestampNonUtc.Month, timestampNonUtc.Day, 0, 0, 0, timestampNonUtc.Kind));
                var end = locationContext.ConvertTimeToUtc(new DateTime(timestampNonUtc.Year, timestampNonUtc.Month, timestampNonUtc.Day, 6, 0, 0, timestampNonUtc.Kind));

                var leakCharacteristic = leakCharacteristicChecker.GetLeakCharacteristic(labelSeries, coldWaterVolume1Delta, start, end);
                if (leakCharacteristic == null)
                {
                    continue;
                }

                var label = labelSeries.Label;
                var detectTimestamp = timestamp;
                var leakUnitValue = leakCharacteristic.Value;
                var flag = leakUnitValue.Value > 0;
                IMeterEventAmplification meterEventAmplification = new LeakMeterEventAmplification(start, end, leakUnitValue);

                yield return new MeterEvent(label, detectTimestamp, flag, meterEventAmplification);
            }
        }

        private static IEnumerable<MeterEvent> GetNewMeterEvents(MeterEvent[] meterEventCandidates, ICollection<MeterEvent> latestMeterEvents)
        {
            var latestMeterEventsByKey = latestMeterEvents.ToDictionary(me => GetKey(me), me => me);
            foreach (var meterEventCandidate in meterEventCandidates)
            {
                var key = GetKey(meterEventCandidate);
                if (!latestMeterEventsByKey.TryGetValue(key, out var meterEvent))
                {
                    if (meterEventCandidate.Flag == true)
                    {
                        yield return meterEventCandidate;
                    }
                }
                else
                {
                    if (meterEventCandidate.DetectTimestamp > meterEvent.DetectTimestamp && meterEventCandidate.Flag != meterEvent.Flag)
                    {
                        yield return meterEventCandidate;
                    }
                }
            }
        }

        private static string GetKey(MeterEvent meterEvent)
        {
            return meterEvent.Label + "_" + meterEvent.Amplification.GetMeterEventType();
        }

    }
}

