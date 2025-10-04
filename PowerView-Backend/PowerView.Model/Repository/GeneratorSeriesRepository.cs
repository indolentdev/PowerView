using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Dapper;

namespace PowerView.Model.Repository
{
    internal class GeneratorSeriesRepository : RepositoryBase, IGeneratorSeriesRepository
    {
        public GeneratorSeriesRepository(IDbContext dbContext)
          : base(dbContext)
        {
        }

        public ICollection<GeneratorSeries> GetGeneratorSeries()
        {
            const string sql = @"
SELECT gs.Label, gs.ObisCode, bl.LabelName AS BaseLabel, bo.ObisCode AS BaseObisCode, gs.CostBreakdownTitle
FROM GeneratorSeries gs
JOIN Label bl ON gs.BaseLabelId = bl.Id
JOIN Obis bo ON gs.BaseObisId = bo.Id;";
            var queryResult = DbContext.QueryTransaction<Db.GeneratorSeriesEx>(sql);

            return queryResult.Select(ToGeneratorSeries).ToList();
        }

        internal static GeneratorSeries ToGeneratorSeries(Db.GeneratorSeriesEx gs)
        {
            return new GeneratorSeries(new SeriesName(gs.Label, gs.ObisCode), new SeriesName(gs.BaseLabel, gs.BaseObisCode), gs.CostBreakdownTitle);
        }

        public void AddGeneratorSeries(GeneratorSeries generatorSeries)
        {
            ArgumentNullException.ThrowIfNull(generatorSeries);

            var baseLabelId = DbContext.QueryTransaction<byte?>("SELECT Id FROM Label WHERE LabelName=@Label;", generatorSeries.BaseSeries)
              .FirstOrDefault();
            var baseObisId = DbContext.QueryTransaction<byte?>("SELECT Id FROM Obis WHERE ObisCode=@ObisCode;", new { ObisCode = (long)generatorSeries.BaseSeries.ObisCode })
              .FirstOrDefault();

            if (baseLabelId == null || baseObisId == null) throw new DataStoreException($"Base series does not exist:{generatorSeries.BaseSeries}");

            var dbGeneratorSeries = new Db.GeneratorSeries
            {
                Label = generatorSeries.Series.Label,
                ObisCode = generatorSeries.Series.ObisCode,
                BaseLabelId = baseLabelId.Value,
                BaseObisId = baseObisId.Value,
                CostBreakdownTitle = generatorSeries.CostBreakdownTitle
            };
            const string sql = "INSERT INTO GeneratorSeries (Label,ObisCode,BaseLabelId,BaseObisId,CostBreakdownTitle) VALUES (@Label,@ObisCode,@BaseLabelId,@BaseObisId,@CostBreakdownTitle);";
            DbContext.ExecuteTransaction(sql, dbGeneratorSeries);
        }

        public void DeleteGeneratorSeries(ISeriesName series)
        {
            ArgumentNullException.ThrowIfNull(series);

            const string sql = "DELETE FROM GeneratorSeries WHERE Label = @Label AND ObisCode = @ObisCode;";
            DbContext.ExecuteTransaction(sql, new { series.Label, ObisCode = (long)series.ObisCode });
        }

        public ICollection<(SeriesName BaseSeries, DateTime LatestTimestamp)> GetBaseSeries()
        {
            long obisCodeQ = ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVatQ;
            long obisCodeH = ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVatH;

            var rows = DbContext.QueryTransaction<RowLocal>(@"
      SELECT l.LabelName AS Label, oc.ObisCode, lol.LatestTimestamp
      FROM LabelObisLive lol
      JOIN Label l ON lol.LabelId=l.Id
      JOIN Obis oc ON lol.ObisId=oc.Id
      WHERE oc.ObisCode=@obisCodeQ OR oc.ObisCode=@obisCodeH;", new { obisCodeQ, obisCodeH });

            return ToSeriesAndTimestamp(rows).ToList();
        }

        private class RowLocal
        {
            public string Label { get; set; }
            public long ObisCode { get; set; }
            public UnixTime LatestTimestamp { get; set; }
        }

        private static IEnumerable<(SeriesName Series, DateTime LatestTimestamp)> ToSeriesAndTimestamp(IEnumerable<RowLocal> rows)
        {
            foreach (var row in rows)
            {
                yield return (new SeriesName(row.Label, row.ObisCode), (DateTime)row.LatestTimestamp);
            }
        }

    }
}
