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
      var labelProfileSet = profileRepository.GetDayProfileSet(start);

      var meterEventCandidates = GetMeterEventCandidates(dateTime, labelProfileSet).ToArray();
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

    private IEnumerable<MeterEvent> GetMeterEventCandidates(DateTime timestamp, LabelProfileSet labelProfileSet)
    {
      foreach (var labelProfile in labelProfileSet)
      {
        if (!labelProfile.GetAllObisCodes().Contains(ObisCode.ColdWaterVolume1Delta))
        {
          continue;
        }

        var timestampNonUtc = timeConverter.ChangeTimeZoneFromUtc(timestamp);
        var start = timeConverter.ChangeTimeZoneToUtc(new DateTime(timestampNonUtc.Year, timestampNonUtc.Month, timestampNonUtc.Day, 0, 0, 0, timestampNonUtc.Kind));
        var end = timeConverter.ChangeTimeZoneToUtc(new DateTime(timestampNonUtc.Year, timestampNonUtc.Month, timestampNonUtc.Day, 6, 0, 0, timestampNonUtc.Kind));

        var leakCharacteristic = labelProfile.GetLeakCharacteristic(ObisCode.ColdWaterVolume1Delta, start, end);
        if (leakCharacteristic == null)
        {
          continue;
        }

        var label = labelProfile.Label;
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

