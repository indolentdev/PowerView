using System;
using System.Collections.Generic;
using System.Linq;
using PowerView.Model;
using PowerView.Model.Repository;

namespace PowerView.Service.EventHub
{
  public class MeterEventDetector : IMeterEventDetector
  {
    private ITimeConverter timeConverter;
    private readonly IProfileRepository profileRepository;
    private readonly IMeterEventRepository meterEventRepository;

    public MeterEventDetector(ITimeConverter timeConverter, IProfileRepository profileRepository, IMeterEventRepository meterEventRepository)
    {
      if (timeConverter == null) throw new ArgumentNullException("timeConverter");
      if (profileRepository == null) throw new ArgumentNullException("profileRepository");
      if (meterEventRepository == null) throw new ArgumentNullException("meterEventRepository");

      this.timeConverter = timeConverter;
      this.profileRepository = profileRepository;
      this.meterEventRepository = meterEventRepository;
    }

    public void DetectMeterEvents(DateTime dateTime)
    {
      if (dateTime.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("dateTime", "Must be UTC");

      var start = dateTime.Subtract(TimeSpan.FromDays(1));
      var preStart = start.Subtract(TimeSpan.FromMinutes(30));
      var labelSeriesSet = profileRepository.GetDayProfileSet(preStart, start, dateTime);

      var timeDivider = DateTimeResolutionDivider.GetResolutionDivider(start, "5-minutes");
      var normalizedLabelSeriesSet = labelSeriesSet.Normalize(timeDivider);
      GenerateSeriesFromCumulative(normalizedLabelSeriesSet);
          
      var meterEventCandidates = GetMeterEventCandidates(dateTime, normalizedLabelSeriesSet).ToArray();
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
        labelSeries.Add(generator.Generate(labelSeries.GetCumulativeSeries()));
      }
    }

    private IEnumerable<MeterEvent> GetMeterEventCandidates(DateTime timestamp, LabelSeriesSet<NormalizedTimeRegisterValue> labelSeriesSet)
    {
      var coldWaterVolume1Delta = ObisCode.ColdWaterVolume1Delta;
      foreach (var labelSeries in labelSeriesSet)
      {
        if (!labelSeries.Contains(coldWaterVolume1Delta))
        {
          continue;
        }

        var timestampNonUtc = timeConverter.ChangeTimeZoneFromUtc(timestamp);
        var start = timeConverter.ChangeTimeZoneToUtc(new DateTime(timestampNonUtc.Year, timestampNonUtc.Month, timestampNonUtc.Day, 0, 0, 0, timestampNonUtc.Kind));
        var end = timeConverter.ChangeTimeZoneToUtc(new DateTime(timestampNonUtc.Year, timestampNonUtc.Month, timestampNonUtc.Day, 6, 0, 0, timestampNonUtc.Kind));

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

