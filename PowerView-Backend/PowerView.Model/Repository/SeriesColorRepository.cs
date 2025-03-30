using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Dapper;

namespace PowerView.Model.Repository
{
    internal class SeriesColorRepository : RepositoryBase, ISeriesColorRepository
    {
        private readonly IObisColorProvider obisColorProvider;
        private Dictionary<SeriesName, string> seriesColorCache;

        public SeriesColorRepository(IDbContext dbContext, IObisColorProvider obisColorProvider)
          : base(dbContext)
        {
            ArgumentNullException.ThrowIfNull(obisColorProvider);

            this.obisColorProvider = obisColorProvider;
        }

        public string GetColorCached(string label, ObisCode obisCode)
        {
            ArgCheck.ThrowIfNullOrEmpty(label);

            PopulateCacheAsNeeded();

            var key = new SeriesName(label, obisCode);
            return seriesColorCache.TryGetValue(key, out var color) ? color : obisColorProvider.GetColor(obisCode);
        }

        public ICollection<SeriesColor> GetSeriesColors()
        {
            return DbContext
              .QueryTransaction<Db.SerieColor>(@"SELECT Id, Label, ObisCode, Color FROM SerieColor;")
              .Select(s => new SeriesColor(new SeriesName(s.Label, s.ObisCode), s.Color))
              .ToList();
        }

        public void SetSeriesColors(IEnumerable<SeriesColor> seriesColors)
        {
            ArgumentNullException.ThrowIfNull(seriesColors);

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
                seriesColorCache = DbContext
                  .QueryTransaction<Db.SerieColor>(@"SELECT Id, Label, ObisCode, Color FROM SerieColor;")
                  .ToDictionary(s => new SeriesName(s.Label, s.ObisCode), s => s.Color);
            }
        }

        private void DeleteAndUpsertSerieColors(IEnumerable<SeriesColor> deleteSeriesColors, IEnumerable<SeriesColor> upsertSeriesColors)
        {
            using var transaction = DbContext.BeginTransaction();
            try
            {
                foreach (var seriesColor in deleteSeriesColors)
                {
                    DbContext.Connection.Execute(
                      "DELETE FROM SerieColor WHERE Label = @Label AND ObisCode = @ObisCode;",
                      new { seriesColor.SeriesName.Label, ObisCode = (long)seriesColor.SeriesName.ObisCode }, transaction);
                }

                foreach (var seriesColor in upsertSeriesColors)
                {
                    var dbSeriesColor = DbContext.Connection.QueryFirstOrDefault<Db.SerieColor>(
                      "SELECT Id, Label, ObisCode, Color FROM SerieColor WHERE Label = @Label AND ObisCode = @ObisCode;",
                      new { seriesColor.SeriesName.Label, ObisCode = (long)seriesColor.SeriesName.ObisCode }, transaction);
                    if (dbSeriesColor != null)
                    {
                        DbContext.Connection.Execute(
                          "UPDATE SerieColor SET Color = @Color WHERE Id = @Id AND Label = @Label AND ObisCode = @ObisCode;",
                          new { seriesColor.Color, dbSeriesColor.Id, dbSeriesColor.Label, dbSeriesColor.ObisCode }, transaction);
                    }
                    else
                    {
                        dbSeriesColor = new Db.SerieColor { Label = seriesColor.SeriesName.Label, ObisCode = seriesColor.SeriesName.ObisCode, Color = seriesColor.Color };
                        DbContext.Connection.Execute(
                          "INSERT INTO SerieColor (Label, ObisCode, Color) VALUES (@Label, @ObisCode, @Color);", dbSeriesColor, transaction);
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
