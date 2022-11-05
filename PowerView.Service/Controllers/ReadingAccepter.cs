using System;
using System.Collections.Generic;
using System.Linq;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.EventHub;

namespace PowerView.Service.Controllers;

public class ReadingAccepter : IReadingAccepter
{
    private readonly ILiveReadingRepository liveReadingRepository;
    private readonly IHub hub;

    public ReadingAccepter(ILiveReadingRepository liveReadingRepository, IHub hub)
    {
        this.liveReadingRepository = liveReadingRepository ?? throw new ArgumentNullException(nameof(liveReadingRepository));
        this.hub = hub ?? throw new ArgumentNullException(nameof(hub));
    }

    public void Accept(IList<Reading> liveReadings)
    {
        var liveReadingsAdd = liveReadings;

        var liveReadingsOk = liveReadings.Where(x => x.GetRegisterValues().Any(RegisterValueOk));
        var liveReadingsToFilter = liveReadings.Except(liveReadingsOk).ToList();

        var liveReadingsFiltered = new List<Reading>();
        if (liveReadingsToFilter.Any())
        {
            foreach (var liveReading in liveReadingsToFilter)
            {
                var filteredRegisterValues = liveReading.GetRegisterValues().Where(RegisterValueOk).ToList();
                if (!filteredRegisterValues.Any())
                {
                    continue;
                }
                liveReadingsFiltered.Add(new Reading(liveReading.Label, liveReading.DeviceId, liveReading.Timestamp, filteredRegisterValues));
            }
            liveReadingsFiltered.AddRange(liveReadingsOk);
            liveReadingsAdd = liveReadingsFiltered;
        }

        liveReadingRepository.Add(liveReadingsAdd);
        hub.Signal(liveReadingsAdd);
    }

    private bool RegisterValueOk(RegisterValue rv)
    {
        return !rv.ObisCode.IsUtilitySpecific;
    }

}
