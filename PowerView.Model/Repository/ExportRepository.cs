using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using log4net;

namespace PowerView.Model.Repository
{
  internal class ExportRepository : RepositoryBase, IExportRepository
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public ExportRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public LabelSeriesSet<TimeRegisterValue> GetLiveCumulativeSeries(DateTime from, DateTime to, IList<string> labels)
    {
      var labelSeriesSet = GetLabelSeriesSet(from, to, labels, "LiveReading", "LiveRegister", oc => oc.IsCumulative);

      return labelSeriesSet;
    }

    private LabelSeriesSet<TimeRegisterValue> GetLabelSeriesSet(DateTime from, DateTime to, IList<string> labels, string readingTable, 
      string registerTable, Func<ObisCode, bool> includeObisCode)
    {
      if (from.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("from", "Must be UTC");
      if (to.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException("to", "Must be UTC");
      if (labels == null) throw new ArgumentNullException("labels");
      if (labels.Count == 0) throw new ArgumentOutOfRangeException("labels", "Must not be emtpy");

      if (log.IsDebugEnabled) log.DebugFormat("Getting LabelSeriesSet using from:{0}, to:{1}",
        from.ToString(CultureInfo.InvariantCulture), to.ToString(CultureInfo.InvariantCulture));

      var sqlQuery = @"
SELECT rea.Label,rea.SerialNumber,rea.Timestamp,reg.ObisCode,reg.Value,reg.Scale,reg.Unit 
FROM {0} AS rea JOIN {1} AS reg ON rea.Id=reg.ReadingId
WHERE rea.Timestamp >= @from AND rea.Timestamp < @to AND rea.Label IN @labels;";
      sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, readingTable, registerTable);
      log.DebugFormat("Querying {0}", readingTable);

      var resultSet = DbContext.QueryTransaction("GetLabelSeriesSet", sqlQuery, new { from, to, labels });

      var labelSeries = GetLabelSeries(resultSet, includeObisCode);

      log.DebugFormat("Assembeled LabelSeriesSet args");
      return new LabelSeriesSet<TimeRegisterValue>(from, to, labelSeries);
    }

    private static List<LabelSeries<TimeRegisterValue>> GetLabelSeries(IEnumerable<dynamic> resultSet, Func<ObisCode, bool> includeObisCode)
    {
      var labelSeries = new List<LabelSeries<TimeRegisterValue>>(5);
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
            new TimeRegisterValue((string)row.SerialNumber, (DateTime)row.Timestamp, (int)row.Value, (short)row.Scale, (Unit)row.Unit)) );
        }
        if (obisCodeToTimeRegisterValues.Count > 0)
        {
          labelSeries.Add(new LabelSeries<TimeRegisterValue>(labelGroup.Key, obisCodeToTimeRegisterValues));
        }
      }
      return labelSeries;
    }

  }
}
