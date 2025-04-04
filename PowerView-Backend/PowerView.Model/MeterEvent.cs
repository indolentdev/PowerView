﻿using System;

namespace PowerView.Model
{
    public class MeterEvent
    {
        public MeterEvent(string label, DateTime detectTimestamp, bool flag, IMeterEventAmplification amplification)
        {
            ArgCheck.ThrowIfNullOrEmpty(label);
            ArgCheck.ThrowIfNotUtc(detectTimestamp);
            ArgumentNullException.ThrowIfNull(amplification);

            Label = label;
            DetectTimestamp = detectTimestamp;
            Flag = flag;
            Amplification = amplification;
        }

        public string Label { get; private set; }
        public DateTime DetectTimestamp { get; private set; }
        public bool Flag { get; private set; }
        public IMeterEventAmplification Amplification { get; private set; }
    }
}

