using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;
using DapperExtensions;

namespace PowerView.Model.Repository
{
  internal class SerieColorRepository : RepositoryBase, ISerieColorRepository
  {
    //    private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IObisColorProvider obisColorProvider;
    private IDictionary<SerieName, string> serieColorCache;

    public SerieColorRepository(IDbContext dbContext, IObisColorProvider obisColorProvider)
      : base(dbContext)
    {
      if (obisColorProvider == null) throw new ArgumentNullException("obisColorProvider");

      this.obisColorProvider = obisColorProvider;
    }

    public string GetColorCached(string label, ObisCode obisCode)
    {
      if (string.IsNullOrEmpty(label)) throw new ArgumentNullException("label");

      PopulateCacheAsNeeded();

      var key = new SerieName(label, obisCode);
      return serieColorCache.ContainsKey(key) ? serieColorCache[key] : obisColorProvider.GetColor(obisCode);
    }

    public ICollection<SerieColor> GetSerieColors()
    {
      var transaction = DbContext.Connection.BeginTransaction();
      try
      {
        var serieColors = DbContext.Connection.GetList<Db.SerieColor>(null, null, transaction, buffered: true);
        transaction.Commit();
        return serieColors.Select(s => new SerieColor(s.Label, s.ObisCode, s.Color)).ToList();
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }
    }

    public void SetSerieColors(IEnumerable<SerieColor> serieColors)
    {
      if (serieColors == null) throw new ArgumentNullException("serieColors");

      serieColorCache = null;

      var deleteSerieColors = new List<SerieColor>();
      var upsertSerieColors = new List<SerieColor>();
      foreach (var serieColor in serieColors)
      {
        if (serieColor.Color == obisColorProvider.GetColor(serieColor.ObisCode))
        {
          deleteSerieColors.Add(serieColor);
        }
        else
        {
          upsertSerieColors.Add(serieColor);
        }
      }

      DeleteAndUpsertSerieColors(deleteSerieColors, upsertSerieColors);
    }

    private void PopulateCacheAsNeeded()
    {
      if (serieColorCache == null)
      {
        var transaction = DbContext.Connection.BeginTransaction();
        try
        {
          var serieColors = DbContext.Connection.GetList<Db.SerieColor>(null, null, transaction, buffered: true);
          transaction.Commit();
          serieColorCache = serieColors.ToDictionary(s => new SerieName(s.Label, s.ObisCode), s => s.Color);
        }
        catch (SqliteException e)
        {
          transaction.Rollback();
          throw DataStoreExceptionFactory.Create(e);
        }
      }
    }

    private void DeleteAndUpsertSerieColors(IEnumerable<SerieColor> deleteSerieColors, IEnumerable<SerieColor> upsertSerieColors)
    {
      var transaction = DbContext.BeginTransaction();
      try
      {
        foreach (var serieColor in deleteSerieColors)
        {
          var labelPredicate = Predicates.Field<Db.SerieColor>(sc => sc.Label, Operator.Eq, serieColor.Label);
          var obisCodePredicate = Predicates.Field<Db.SerieColor>(sc => sc.ObisCode, Operator.Eq, (long)serieColor.ObisCode);
          DbContext.Connection.Delete<Db.SerieColor>(Predicates.Group(GroupOperator.And, labelPredicate, obisCodePredicate), transaction, null);
        }

        foreach (var serieColor in upsertSerieColors)
        {
          var labelPredicate = Predicates.Field<Db.SerieColor>(sc => sc.Label, Operator.Eq, serieColor.Label);
          var obisCodePredicate = Predicates.Field<Db.SerieColor>(sc => sc.ObisCode, Operator.Eq, (long)serieColor.ObisCode);
          var dbSerieColor = DbContext.Connection.GetList<Db.SerieColor>(Predicates.Group(GroupOperator.And, labelPredicate, obisCodePredicate), null, transaction, buffered:true).SingleOrDefault();
          if (dbSerieColor != null)
          {
            dbSerieColor.Color = serieColor.Color;
            DbContext.Connection.Update(dbSerieColor, transaction);
          }
          else
          {
            dbSerieColor = new Db.SerieColor { Label = serieColor.Label, ObisCode = serieColor.ObisCode, Color = serieColor.Color };
            DbContext.Connection.Insert(dbSerieColor, transaction);
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
