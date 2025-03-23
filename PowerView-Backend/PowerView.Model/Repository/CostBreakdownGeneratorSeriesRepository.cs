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
  internal class CostBreakdownGeneratorSeriesRepository : RepositoryBase, ICostBreakdownGeneratorSeriesRepository
  {
    public CostBreakdownGeneratorSeriesRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public IReadOnlyList<CostBreakdownGeneratorSeries> GetCostBreakdownGeneratorSeries()
    {
      const string sql = @"
      SELECT gs.Label, gs.ObisCode, bl.LabelName AS BaseLabel, bo.ObisCode AS BaseObisCode, gs.CostBreakdownTitle, cb.Id, cb.Title, cb.Currency, cb.Vat, cbe.CostBreakdownId, cbe.FromDate, cbe.ToDate, cbe.Name, cbe.StartTime, cbe.EndTime, cbe.Amount 
      FROM GeneratorSeries gs
      JOIN Label bl ON gs.BaseLabelId = bl.Id
      JOIN Obis bo ON gs.BaseObisId = bo.Id
      LEFT JOIN CostBreakdown cb ON gs.CostBreakdownTitle=cb.Title
      LEFT JOIN CostBreakdownEntry cbe ON cb.Id=cbe.CostBreakdownId
      ORDER BY gs.Label, gs.ObisCode, cb.Id, cbe.CostBreakdownId;";

      var cache = new Dictionary<SeriesName, (Db.GeneratorSeriesEx GeneratorSeries, Db.CostBreakdown CostBreakdown, List<Db.CostBreakdownEntry> CostBreakdownEntries)>();
      Func<Db.GeneratorSeriesEx, Db.CostBreakdown, Db.CostBreakdownEntry, (Db.GeneratorSeriesEx GeneratorSeries, Db.CostBreakdown CostBreakdown, List<Db.CostBreakdownEntry> CostBreakdownEntries)> map = (gs, cb, cbe) =>
      {
        var seriesName = new SeriesName(gs.Label, gs.ObisCode);
        if (!cache.ContainsKey(seriesName))
        {
          cache.Add(seriesName, (gs, cb, new List<Db.CostBreakdownEntry>()));
        }

        var cacheItem = cache[seriesName];
        if (cbe != null) cacheItem.CostBreakdownEntries.Add(cbe);

        return cacheItem;
      };

      DbContext.QueryMultiMappingTransaction(sql, map, null, "Id,CostBreakdownId");

      var result = new List<CostBreakdownGeneratorSeries>();
      foreach (var item in cache.Values)
      {
        if (item.CostBreakdown == null) continue;

        var costBreakdown = CostBreakdownRepository.ToCostBreakdown(item.CostBreakdown, item.CostBreakdownEntries);
        var generatorSeries = GeneratorSeriesRepository.ToGeneratorSeries(item.GeneratorSeries);
        result.Add(new CostBreakdownGeneratorSeries(costBreakdown, generatorSeries));
      }

      return result;
    }

  }
}
