using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;
using DapperExtensions;

namespace PowerView.Model.Repository
{
  internal class SeriesColorRepository : RepositoryBase, ISeriesColorRepository
  {
    //    private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IObisColorProvider obisColorProvider;
    private IDictionary<SeriesName, string> seriesColorCache;

    public SeriesColorRepository(IDbContext dbContext, IObisColorProvider obisColorProvider)
      : base(dbContext)
    {
      if (obisColorProvider == null) throw new ArgumentNullException("obisColorProvider");

      this.obisColorProvider = obisColorProvider;
    }

    public string GetColorCached(string label, ObisCode obisCode)
    {
      if (string.IsNullOrEmpty(label)) throw new ArgumentNullException("label");

      PopulateCacheAsNeeded();

      var key = new SeriesName(label, obisCode);
      return seriesColorCache.ContainsKey(key) ? seriesColorCache[key] : obisColorProvider.GetColor(obisCode);
    }

    public ICollection<SeriesColor> GetSeriesColors()
    {
      var transaction = DbContext.Connection.BeginTransaction();
      try
      {
        var serieColors = DbContext.Connection.GetList<Db.SerieColor>(null, null, transaction, buffered: true);
        transaction.Commit();
        return serieColors.Select(s => new SeriesColor(new SeriesName(s.Label, s.ObisCode), s.Color)).ToList();
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }
    }

    public void SetSeriesColors(IEnumerable<SeriesColor> seriesColors)
    {
      if (seriesColors == null) throw new ArgumentNullException("seriesColors");

      seriesColorCache = null;

      var deleteSeriesColors = new List<SeriesColor>();
      var upsertSeriesColors = new List<SeriesColor>();
      foreach (var seriesColor in seriesColors)
      {
        if (seriesColor.Color == obisColorProvider.GetColor(seriesColor.SeriesName.ObisCode))
        {
          deleteSeriesColors.Add(seriesColor);
        }
        else
        {
          upsertSeriesColors.Add(seriesColor);
        }
      }

      DeleteAndUpsertSerieColors(deleteSeriesColors, upsertSeriesColors);
    }

    private void PopulateCacheAsNeeded()
    {
      if (seriesColorCache == null)
      {
        var transaction = DbContext.Connection.BeginTransaction();
        try
        {
          var serieColors = DbContext.Connection.GetList<Db.SerieColor>(null, null, transaction, buffered: true);
          transaction.Commit();
          seriesColorCache = serieColors.ToDictionary(s => new SeriesName(s.Label, s.ObisCode), s => s.Color);
        }
        catch (SqliteException e)
        {
          transaction.Rollback();
          throw DataStoreExceptionFactory.Create(e);
        }
      }
    }

    private void DeleteAndUpsertSerieColors(IEnumerable<SeriesColor> deleteSeriesColors, IEnumerable<SeriesColor> upsertSeriesColors)
    {
      var transaction = DbContext.BeginTransaction();
      try
      {
        foreach (var seriesColor in deleteSeriesColors)
        {
          var labelPredicate = Predicates.Field<Db.SerieColor>(sc => sc.Label, Operator.Eq, seriesColor.SeriesName.Label);
          var obisCodePredicate = Predicates.Field<Db.SerieColor>(sc => sc.ObisCode, Operator.Eq, (long)seriesColor.SeriesName.ObisCode);
          DbContext.Connection.Delete<Db.SerieColor>(Predicates.Group(GroupOperator.And, labelPredicate, obisCodePredicate), transaction, null);
        }

        foreach (var seriesColor in upsertSeriesColors)
        {
          var labelPredicate = Predicates.Field<Db.SerieColor>(sc => sc.Label, Operator.Eq, seriesColor.SeriesName.Label);
          var obisCodePredicate = Predicates.Field<Db.SerieColor>(sc => sc.ObisCode, Operator.Eq, (long)seriesColor.SeriesName.ObisCode);
          var dbSeriesColor = DbContext.Connection.GetList<Db.SerieColor>(Predicates.Group(GroupOperator.And, labelPredicate, obisCodePredicate), null, transaction, buffered:true).SingleOrDefault();
          if (dbSeriesColor != null)
          {
            dbSeriesColor.Color = seriesColor.Color;
            DbContext.Connection.Update(dbSeriesColor, transaction);
          }
          else
          {
            dbSeriesColor = new Db.SerieColor { Label = seriesColor.SeriesName.Label, ObisCode = seriesColor.SeriesName.ObisCode, Color = seriesColor.Color };
            DbContext.Connection.Insert(dbSeriesColor, transaction);
          }
        }

        transaction.Commit();
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }
    }
  }

}
