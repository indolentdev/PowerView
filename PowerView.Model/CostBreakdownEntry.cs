﻿using System;
using System.Globalization;

namespace PowerView.Model
{
  public class CostBreakdownEntry
  {
    public CostBreakdownEntry(DateTime fromDate, DateTime toDate, string name, int startTime, int endTime, double amount)
    {
      if (fromDate.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(fromDate), $"Must be UTC. Was:{fromDate.Kind}");
      if (toDate.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(toDate), $"Must be UTC. Was:{toDate.Kind}");
      if (toDate <= fromDate) throw new ArgumentOutOfRangeException(nameof(toDate), "Must be greater than fromDate");
      if (string.IsNullOrEmpty(name)) throw new ArgumentOutOfRangeException(nameof(name), "Must not be null or empty");
      if (startTime < 0 || startTime > 22) throw new ArgumentOutOfRangeException(nameof(startTime), $"Must be between 0 and 22. Was:{startTime}");
      if (endTime < 1 || endTime > 23) throw new ArgumentOutOfRangeException(nameof(endTime), $"Must be between 1 and 23. Was:{endTime}");
      if (endTime <= startTime) throw new ArgumentOutOfRangeException(nameof(endTime), "Must be greater than startTime");

      FromDate = fromDate;
      ToDate = toDate;
      Name = name;
      StartTime = startTime;
      EndTime = endTime;
      Amount = amount;
    }

    public DateTime FromDate { get; private set; }
    public DateTime ToDate { get; private set; }
    public string Name { get; private set; }
    public int StartTime { get; private set; }
    public int EndTime { get; private set; }
    public double Amount { get; private set; }

    public override string ToString()
    {
      return string.Format(CultureInfo.InvariantCulture, "[CostBreakDownEntry: FromDate={0}, ToDate={1}, Name={2}, StartTime={3}, EndTime={4}, Amount={5}]", 
        FromDate.ToString("O"), ToDate.ToString("O"), Name, StartTime, EndTime, Amount);
    }

  }
}

