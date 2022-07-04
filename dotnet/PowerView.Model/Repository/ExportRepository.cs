using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PowerView.Model.Repository
{
  internal class ExportRepository : RepositoryBase, IExportRepository
  {
    public ExportRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public TimeRegisterValueLabelSeriesSet GetLiveCumulativeSeries(DateTime from, DateTime to, IList<string> labels)
    {
      var labelSeriesSet = GetLabelSeriesSet(from, to, labels, "LiveReading", "LiveRegister", oc => oc.IsCumulative);

      return labelSeriesSet;
    }

    private TimeRegisterValueLabelSeriesSet GetLabelSeriesSet(DateTime from, DateTime to, IList<string> labels, string readingTable, 
      string registerTable, Func<ObisCode, bool> includeObisCode)
    {
      if (from.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("from", "Must be UTC");
      if (to.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("to", "Must be UTC");
      if (labels == null) throw new ArgumentNullException("labels");
      if (labels.Count == 0) throw new ArgumentOutOfRangeException("labels", "Must not be emtpy");

      var sqlQuery = @"
SELECT rea.Label,rea.DeviceId,rea.Timestamp,reg.ObisCode,reg.Value,reg.Scale,reg.Unit 
FROM {0} AS rea JOIN {1} AS reg ON rea.Id=reg.ReadingId
WHERE rea.Timestamp >= @from AND rea.Timestamp <= @to AND rea.Label IN @labels;";
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, readingTable, registerTable);
      var resultSet = DbContext.QueryTransaction(sqlQuery, new { from, to, labels });

      var labelSeries = GetLabelSeries(resultSet, includeObisCode);

      return new TimeRegisterValueLabelSeriesSet(from, to, labelSeries);
    }

    private static List<TimeRegisterValueLabelSeries> GetLabelSeries(IEnumerable<dynamic> resultSet, Func<ObisCode, bool> includeObisCode)
    {
      var labelSeries = new List<TimeRegisterValueLabelSeries>(5);
      var groupedByLabel = resultSet.GroupBy(r => { string s = r.Label; return s; }, r => r);
      foreach (IGrouping<string, dynamic> labelGroup in groupedByLabel)
      {
        var obisCodeToTimeRegisterValues = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>(8);
        var groupedByObisCode = labelGroup.GroupBy(r => { ObisCode oc = r.ObisCode; return oc; }, r => r);
        foreach (IGrouping<ObisCode, dynamic> obisCodeGroup in groupedByObisCode)
        {
          var obisCode = obisCodeGroup.Key;

          if (!includeObisCode(obisCode))
          {
            continue;
          }

          obisCodeToTimeRegisterValues.Add(obisCode, obisCodeGroup.Select(row =>
            new TimeRegisterValue((string)row.DeviceId, (DateTime)row.Timestamp, (int)row.Value, (short)row.Scale, (Unit)row.Unit)) );
        }
        if (obisCodeToTimeRegisterValues.Count > 0)
        {
          labelSeries.Add(new TimeRegisterValueLabelSeries(labelGroup.Key, obisCodeToTimeRegisterValues));
        }
      }
      return labelSeries;
    }

  }
}