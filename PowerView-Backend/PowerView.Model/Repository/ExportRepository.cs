using System.Globalization;

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
            ArgCheck.ThrowIfNotUtc(from);
            ArgCheck.ThrowIfNotUtc(to);
            ArgumentNullException.ThrowIfNull(labels);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(labels.Count, 0, nameof(labels));

            var sqlQuery = @"
SELECT lbl.LabelName AS Label,dev.DeviceName AS DeviceId,rea.Timestamp,o.ObisCode,reg.Value,reg.Scale,reg.Unit 
FROM {0} AS rea JOIN Label AS lbl ON rea.LabelId=lbl.Id JOIN Device AS dev ON rea.DeviceId=dev.Id JOIN {1} AS reg ON rea.Id=reg.ReadingId JOIN Obis o ON reg.ObisId=o.Id
WHERE rea.Timestamp >= @from AND rea.Timestamp <= @to AND lbl.LabelName IN @labels;";
            sqlQuery = string.Format(CultureInfo.InvariantCulture, sqlQuery, readingTable, registerTable);
            var resultSet = DbContext.QueryTransaction<RowLocal>(sqlQuery, new { from = (UnixTime)from, to = (UnixTime)to, labels });

            var labelSeries = GetLabelSeries(resultSet, includeObisCode);

            return new TimeRegisterValueLabelSeriesSet(from, to, labelSeries);
        }

        private static List<TimeRegisterValueLabelSeries> GetLabelSeries(IEnumerable<RowLocal> resultSet, Func<ObisCode, bool> includeObisCode)
        {
            var labelSeries = new List<TimeRegisterValueLabelSeries>(5);
            var groupedByLabel = resultSet.GroupBy(r => { string s = r.Label; return s; }, r => r);
            foreach (IGrouping<string, RowLocal> labelGroup in groupedByLabel)
            {
                var obisCodeToTimeRegisterValues = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>(8);
                var groupedByObisCode = labelGroup.GroupBy(r => { ObisCode oc = r.ObisCode; return oc; }, r => r);
                foreach (IGrouping<ObisCode, RowLocal> obisCodeGroup in groupedByObisCode)
                {
                    var obisCode = obisCodeGroup.Key;

                    if (!includeObisCode(obisCode))
                    {
                        continue;
                    }

                    obisCodeToTimeRegisterValues.Add(obisCode, obisCodeGroup.Select(row =>
                      new TimeRegisterValue(row.DeviceId, row.Timestamp, row.Value, row.Scale, (Unit)row.Unit)));
                }
                if (obisCodeToTimeRegisterValues.Count > 0)
                {
                    labelSeries.Add(new TimeRegisterValueLabelSeries(labelGroup.Key, obisCodeToTimeRegisterValues));
                }
            }
            return labelSeries;
        }

        private class RowLocal
        {
            public string Label { get; set; }
            public string DeviceId { get; set; }
            public long ObisCode { get; set; }
            public UnixTime Timestamp { get; set; }
            public int Value { get; set; }
            public short Scale { get; set; }
            public byte Unit { get; set; }
        }

    }
}
