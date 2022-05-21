/*
#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PowerView.Model;
using PowerView.Model.Repository;

using OC = PowerView.Model.ObisCode;
using ActSG = PowerView.TestDataGenerator.ActualSerieGenerator;
using AccSG = PowerView.TestDataGenerator.AccumulatingSerieGenerator;
using SumSG = PowerView.TestDataGenerator.SumSerieGenerator;

namespace PowerView
{
  internal class TestDataGenerator
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly ILifetimeScope scope;

    public TestDataGenerator(ILifetimeScope scope)
    {
      if (scope == null) throw new ArgumentNullException("scope");
      this.scope = scope;
    }

    public void Invoke()
    {
      GenerateDayProfile();
      GenerateMeterEvents();

      log.InfoFormat("Done generating test data");
    }

    private void GenerateMeterEvents()
    { 
      using (var scope2 = scope.BeginLifetimeScope())
      {
        log.InfoFormat("Generating meter events test data..");
        var repo = scope2.Resolve<IMeterEventRepository>();

        var eventGenerators = GetEventGenerators();
        var rnd = new Random();
        foreach (var item in eventGenerators)
        {
          var meterEvent = GenerateMeterEvents(rnd, item.Key, item.Value).ToArray();
          repo.AddMeterEvents(meterEvent);
        }
      }
    }

    private void GenerateDayProfile()
    { 
      using (var scope2 = scope.BeginLifetimeScope())
      {
        var profileRepo = scope2.Resolve<IProfileRepository>();
        var now = DateTime.Now;
        var start = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0, now.Kind).ToUniversalTime();
        var labelSeriesSet = profileRepo.GetDayProfileSet(start, start, start.AddDays(1));
        if (labelSeriesSet.Any())
        {
          return;
        }

        log.InfoFormat("Generating day profile test data..");
        var repo = scope2.Resolve<ILiveReadingRepository>();

        var serieGenerators = GetSerieGenerators();
        var rnd = new Random();
        foreach (var item in serieGenerators)
        {
          var liveReadings = GenerateLiveReadings(rnd, item.Key, item.Value).ToArray();
          repo.Add(liveReadings);
        }
      }
    }

    private static IDictionary<string, SerieGenerator[]> GetSerieGenerators()
    {
      var result = new Dictionary<string, SerieGenerator[]>();

      var mainActualPowerP14 = new ActSG { Unit=Unit.Watt, Scale=0, BaseValue=400, BaseOffset=1100, MinValue=500, MaxValue=5000, ObisCode = OC.ElectrActualPowerP14 };
      var mainActiveEnergyA14 = new AccSG { Unit=Unit.WattHour, Scale=0, ObisCode=OC.ElectrActiveEnergyA14, BackingGenerator=mainActualPowerP14 };
      var mainActualPowerP23L1 = new ActSG { Unit=Unit.Watt, Scale=0, BaseValue=40, BaseOffset=300,  MinValue=100, MaxValue=1000, ObisCode = OC.ElectrActualPowerP23L1 };
      var mainActualPowerP23L2 = new ActSG { Unit = Unit.Watt, Scale = 0, BaseValue = 40, BaseOffset = 300, MinValue = 100, MaxValue = 1000, ObisCode = OC.ElectrActualPowerP23L2 };
      var mainActualPowerP23L3 = new ActSG { Unit = Unit.Watt, Scale = 0, BaseValue = 40, BaseOffset = 300, MinValue = 100, MaxValue = 1000, ObisCode = OC.ElectrActualPowerP23L3 };
      var mainActualPowerP23 = new SumSG { Unit = Unit.Watt, Scale = 0, ObisCode = OC.ElectrActualPowerP23, BackingGenerators = new [] {
          mainActualPowerP23L1, mainActualPowerP23L2, mainActualPowerP23L3
        } };
      var mainActiveEnergyA23 = new AccSG { Unit=Unit.WattHour, Scale=0, ObisCode=OC.ElectrActiveEnergyA23, BackingGenerator=mainActualPowerP23 };
      result.Add("Main", new SerieGenerator[] { mainActualPowerP14, mainActiveEnergyA14, mainActualPowerP23L1, mainActualPowerP23L2, mainActualPowerP23L3, mainActualPowerP23, mainActiveEnergyA23 });

      var heaterActualPowerP14 = new ActSG { Unit=Unit.Watt, Scale=0, BaseValue=50, BaseOffset=500, MinValue=100, MaxValue=4000, ObisCode = OC.ElectrActualPowerP14 };
      var heaterActiveEnergyA14 = new AccSG { Unit=Unit.WattHour, Scale=0, ObisCode=OC.ElectrActiveEnergyA14, BackingGenerator=heaterActualPowerP14 };
      result.Add("Heater", new SerieGenerator[] { heaterActualPowerP14, heaterActiveEnergyA14 });

      var invActualPowerP23 = new ActSG { Unit=Unit.Watt, Scale=0, BaseValue=150, BaseOffset=900,  MinValue=500, MaxValue=3000, ObisCode = OC.ElectrActualPowerP23 };
      var invActiveEnergyA23 = new AccSG { Unit=Unit.WattHour, Scale=0, ObisCode=OC.ElectrActiveEnergyA23, BackingGenerator=invActualPowerP23 };
      result.Add("Inverter", new SerieGenerator[] { invActualPowerP23, invActiveEnergyA23 });

      var coldWaterFlow = new ActSG { Unit=Unit.CubicMetrePrHour, Scale=-3, BaseValue=5, BaseOffset=200,  MinValue=25, MaxValue=600, ObisCode = OC.ColdWaterFlow1 };
      var coldWaterVolume = new AccSG { Unit=Unit.CubicMetre, Scale=-3, ObisCode=OC.ColdWaterVolume1, BackingGenerator=coldWaterFlow };
      result.Add("Water", new SerieGenerator[] { coldWaterFlow, coldWaterVolume });

      var hotWaterFlow = new ActSG { Unit = Unit.CubicMetrePrHour, Scale = -3, BaseValue = 5, BaseOffset = 200, MinValue = 25, MaxValue = 600, ObisCode = OC.HotWaterFlow1 };
      var hotWaterVolume = new AccSG { Unit = Unit.CubicMetre, Scale = -3, ObisCode = OC.HotWaterVolume1, BackingGenerator = hotWaterFlow };
      result.Add("HotWater", new SerieGenerator[] { hotWaterFlow, hotWaterVolume });

      var heatFlow = new ActSG { Unit=Unit.CubicMetrePrHour, Scale=-3, BaseValue=5, BaseOffset=200,  MinValue=25, MaxValue=600, ObisCode = OC.HeatEnergyFlow1 };
      var heatVolume = new AccSG { Unit=Unit.CubicMetre, Scale=-3, ObisCode=OC.HeatEnergyVolume1, BackingGenerator=heatFlow };
      var heatPower = new ActSG { Unit=Unit.Watt, Scale=0, BaseValue=400, BaseOffset=1100, MinValue=500, MaxValue=5000, ObisCode = OC.HeatEnergyPower1 };
      var heatEnergy = new AccSG { Unit=Unit.WattHour, Scale=0, ObisCode=OC.HeatEnergyEnergy1, BackingGenerator=heatPower };
      var heatT1 = new ActSG { Unit=Unit.DegreeCelsius, Scale=-1, BaseValue=650, BaseOffset=100, MinValue=450, MaxValue=960, ObisCode = OC.HeatEnergyFlowTemperature };
      var heatT2 = new ActSG { Unit=Unit.DegreeCelsius, Scale=-1, BaseValue=320, BaseOffset=100, MinValue=210, MaxValue=720, ObisCode = OC.HeatEnergyReturnTemperature };
      result.Add("Heat", new SerieGenerator[] { heatFlow, heatVolume, heatPower, heatEnergy, heatT1, heatT2 });

      var convTemp = new ActSG { Unit=Unit.DegreeCelsius, Scale=-1, BaseValue=150, BaseOffset=150, MinValue=-150, MaxValue=450, ObisCode = OC.RoomTemperature };
      var convRh = new ActSG { Unit=Unit.Percentage, Scale=-1, BaseValue=50, BaseOffset=100, MinValue=320, MaxValue=820, ObisCode = OC.RoomRelativeHumidity };
      result.Add("Conservatory", new SerieGenerator[] { convTemp, convRh });

      var hallTemp = new ActSG { Unit=Unit.DegreeCelsius, Scale=-1, BaseValue=50, BaseOffset=90, MinValue=180, MaxValue=350, ObisCode = OC.RoomTemperature };
      var hallRh = new ActSG { Unit=Unit.Percentage, Scale=-1, BaseValue=50, BaseOffset=100, MinValue=320, MaxValue=820, ObisCode = OC.RoomRelativeHumidity };
      result.Add("Hall", new SerieGenerator[] { hallTemp, hallRh });

      var relayUnit1 = new ActSG { Unit=Unit.NoUnit, Scale=0, BaseValue=0, BaseOffset=1, MinValue=0, MaxValue=1, ObisCode = "0.1.96.3.10.255" };
      var relayUnit2 = new ActSG { Unit = Unit.NoUnit, Scale = 0, BaseValue = 0, BaseOffset = 1, MinValue = 0, MaxValue = 1, ObisCode = "0.2.96.3.10.255" };
      result.Add("Relay", new SerieGenerator[] { relayUnit1, relayUnit2 });

      return result;
    }

    private static IEnumerable<LiveReading> GenerateLiveReadings(Random rnd, string label, SerieGenerator[] serieGenerators)
    {
      var timestamps = GetReadingTimestamps(DateTime.Now).ToArray();
      foreach (var timestamp in timestamps)
      {
        var registerValues = GenerateRegisterValues(rnd, serieGenerators).ToArray();
        var liveReading = new LiveReading(label, "1", timestamp, registerValues);
        yield return liveReading;
      }
    }

    private static IEnumerable<DateTime> GetReadingTimestamps(DateTime dt)
    {
      var dtStartMinute = dt.Minute % 5;
      if (dtStartMinute >= 5) dtStartMinute = 4;
      var dtStart = new DateTime(dt.Year, dt.Month, dt.Day, 0, dtStartMinute, dt.Second, 0, dt.Kind);
      var dtValue = dtStart;

      var increment = TimeSpan.FromMinutes(5);
      var dayTimeSpan = TimeSpan.FromHours(23) + TimeSpan.FromMinutes(19); // DayProfile starts reading 30min back.. 41 mins is a safe margin
      while (dtValue.Day == dtStart.Day && dtValue < dtStart + dayTimeSpan)
      {
        yield return dtValue.ToUniversalTime();
        dtValue += increment;
      }
    }

    private static IEnumerable<RegisterValue> GenerateRegisterValues(Random rnd, SerieGenerator[] serieGenerators)
    {
      foreach (var serieGenerator in serieGenerators)
      {
        serieGenerator.GenerateNextValue(rnd);
        var value = (int)serieGenerator.GetValue();
        yield return new RegisterValue(serieGenerator.ObisCode, value, serieGenerator.Scale, serieGenerator.Unit);
      }
    }

    private static IDictionary<string, EventGenerator[]> GetEventGenerators()
    {
      var result = new Dictionary<string, EventGenerator[]>();

      var leakGenerator = new LeakEventGenerator { Unit=Unit.CubicMetre };
      result.Add("Water", new EventGenerator[] { leakGenerator });

      return result;
    }

    private static IEnumerable<MeterEvent> GenerateMeterEvents(Random rnd, string label, EventGenerator[] eventGenerators)
    {
      var timestamps = GetEventTimestamps(DateTime.Now).ToArray();
      foreach (var timestamp in timestamps)
      {
        foreach (var eventGenerator in eventGenerators)
        {
          var amplification = eventGenerator.GenerateAmplification(rnd, timestamp);
          if (amplification == null) continue;
          var meterEvent = new MeterEvent(label, timestamp, eventGenerator.Flag, amplification);
          yield return meterEvent;
        }
      }
    }

    private static IEnumerable<DateTime> GetEventTimestamps(DateTime dt)
    {
      var dtHour = 0;
      var dtStart = new DateTime(dt.Year, dt.Month, dt.Day, dtHour, dt.Minute, dt.Second, 0, dt.Kind);
      var dtValue = dtStart;

      var increment = TimeSpan.FromHours(1);
      while (dtHour < 24)
      {
        yield return dtValue.ToUniversalTime();
        dtValue += increment;
        dtHour += 1;
      }
    }

    internal abstract class EventGenerator
    {
      internal abstract IMeterEventAmplification GenerateAmplification(Random rnd, DateTime dt);
      public bool Flag { get; protected set; }
    }

    internal class LeakEventGenerator : EventGenerator
    { 
      public Unit Unit { get; set; }

      internal override IMeterEventAmplification GenerateAmplification(Random rnd, DateTime dt)
      {
        if (!Flag)
        {
          if (rnd.NextDouble() < 0.4) return null;
          Flag = !Flag;
          return CreateAmplification(dt, rnd.NextDouble() * 10);
        }
        else
        { 
          if (rnd.NextDouble() < 0.85) return null;
          Flag = !Flag;
          return CreateAmplification(dt, rnd.NextDouble());
        }
      }

      private IMeterEventAmplification CreateAmplification(DateTime dt, double value)
      {
        return new LeakMeterEventAmplification(dt, dt, new UnitValue(value, Unit));
      }
    }

    internal abstract class SerieGenerator
    {
      public Unit Unit { get; set; }
      public short Scale { get; set; }
      public OC ObisCode { get; set; }

      public abstract void GenerateNextValue(Random rnd);
      public abstract double GetValue();

      public override string ToString()
      {
        return string.Format("[Spec: Unit={0}, Scale={1}, ObisCode={2}]", Unit, Scale, ObisCode);
      }
    }

    internal class ActualSerieGenerator : SerieGenerator
    {
      public ActualSerieGenerator()
      {
        Offset = double.NaN;
      }

      public double BaseValue { get; set; }
      private double baseOffset;
      public double BaseOffset
      {
        get { return baseOffset; }
        set 
        { 
          baseOffset = value;
          if (double.IsNaN(Offset))
          {
            Offset = baseOffset;
          }
        }
      }
      public double Offset { get; set; }
      public double MinValue { get; set; }
      public double MaxValue { get; set; }

      private double value;
      public override void GenerateNextValue(Random rnd)
      {
        var exhaustCounter = 0;
        while (exhaustCounter < 1000)
        {
          exhaustCounter++;

          var factor = ((double)rnd.Next(52500, 150000)) / 100000;
          var nextOffset = Offset*0.3*factor + Offset*0.7;
          var nextValue = BaseValue + nextOffset;

          var midValue = (MaxValue - MinValue) / 2;
          var midValueTenth = midValue * 0.1d;
          if (nextValue > MaxValue-midValueTenth)
          {
            Offset = Offset-BaseOffset;
            continue;
          }
          if (nextValue < MinValue+midValueTenth )
          {
            Offset = Offset+BaseOffset;
            continue;
          }

          Offset = nextOffset;
          value = nextValue;
          return;
        }

        throw new OverflowException("Not possible to generate next value. Provide other specs. " + ToString());
      }

      public override double GetValue()
      {        
        return value;
      }
    }

    internal class AccumulatingSerieGenerator : SerieGenerator
    {
      public SerieGenerator BackingGenerator { get; set; }

      private double value;

      public override void GenerateNextValue(Random rnd)
      {
        var actualValue = BackingGenerator.GetValue();

        if (Unit == Unit.WattHour && BackingGenerator.Unit == Unit.Watt && Scale == 0 && BackingGenerator.Scale == Scale)
        {
          value += (actualValue / 12); // accumulate 5 min "pieces" .. makes 12 pr 60 mins.
          return;
        }

        if (Unit == Unit.CubicMetre && BackingGenerator.Unit == Unit.CubicMetrePrHour && Scale == -3 && BackingGenerator.Scale == Scale)
        {
          value += (actualValue / 12); // accumulate 5 min "pieces" .. makes 12 pr 60 mins.
          return;
        }

        var msg = string.Format(System.Globalization.CultureInfo.InvariantCulture, 
          "Unable to generate next value. {0}. Backing {1}", this, BackingGenerator);
        throw new NotSupportedException(msg);
      }

      public override double GetValue()
      {
        return value;
      }
    }

    internal class SumSerieGenerator : SerieGenerator
    {
      public SerieGenerator[] BackingGenerators { get; set; }

      private double value;

      public override void GenerateNextValue(Random rnd)
      {
        var actualValue = BackingGenerators.Sum(x => x.GetValue());
        value = actualValue;
      }

      public override double GetValue()
      {
        return value;
      }
    }
  }

}

#endif
*/