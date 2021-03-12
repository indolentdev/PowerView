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

    public MeterEventDetector(IProfileRepository profileRepository, IMeterEventRepository meterEventRepository, ILocationContext locationContext)
    {
      if (profileRepository == null) throw new ArgumentNullException("profileRepository");
      if (meterEventRepository == null) throw new ArgumentNullException("meterEventRepository");
      if (locationContext == null) throw new ArgumentNullException("locationContext");

      this.profileRepository = profileRepository;
      this.meterEventRepository = meterEventRepository;
      this.locationContext = locationContext;
    }

    public void DetectMeterEvents(DateTime timestamp)
    {
      if (timestamp.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(timestamp), "Must be UTC");

      var dateLocal = locationContext.ConvertTimeFromUtc(timestamp);
      var localMidnightAsUtc = dateLocal.Date.ToUniversalTime();

      var end = localMidnightAsUtc.AddDays(1);
      var preStart = localMidnightAsUtc.Subtract(TimeSpan.FromMinutes(30));
      var labelSeriesSet = profileRepository.GetDayProfileSet(preStart, localMidnightAsUtc, end);

      var timeDivider = new DateTimeHelper(locationContext.TimeZoneInfo, localMidnightAsUtc).GetDivider("5-minutes");
      var normalizedLabelSeriesSet = labelSeriesSet.Normalize(timeDivider);
      GenerateSeriesFromCumulative(normalizedLabelSeriesSet);
          
      var meterEventCandidates = GetMeterEventCandidates(timestamp, localMidnightAsUtc, normalizedLabelSeriesSet).ToArray();
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

    private void GenerateSeriesFromCumulative(LabelSeriesSet<NormalizedTimeRegisterValue> labelSeriesSet)
    {
      foreach (var labelSeries in labelSeriesSet)
      {
        var generator = new SeriesFromCumulativeGenerator();
        labelSeries.Add(generator.GenerateOld(labelSeries.GetCumulativeSeries()));
      }
    }

    private IEnumerable<MeterEvent> GetMeterEventCandidates(DateTime timestamp, DateTime localMidnightAsUtc, LabelSeriesSet<NormalizedTimeRegisterValue> labelSeriesSet)
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

        var leakChecker = new LeakCharacteristicChecker();
        var leakCharacteristic = leakChecker.GetLeakCharacteristic(labelSeries, coldWaterVolume1Delta, start, end);
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

    private IEnumerable<MeterEvent> GetNewMeterEvents(MeterEvent[] meterEventCandidates, ICollection<MeterEvent> latestMeterEvents)
    {
      var latestMeterEventsByKey = latestMeterEvents.ToDictionary(me => GetKey(me), me => me);
      foreach (var meterEventCandidate in meterEventCandidates)
      {
        var key = GetKey(meterEventCandidate);
        if (!latestMeterEventsByKey.ContainsKey(key))
        {
          if (meterEventCandidate.Flag == true)
          {
            yield return meterEventCandidate;
          }
        }
        else
        {
          var meterEvent = latestMeterEventsByKey[key];
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

