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
    internal class ImportRepository : RepositoryBase, IImportRepository
    {
        public ImportRepository(IDbContext dbContext)
          : base(dbContext)
        {
        }

        public ICollection<Import> GetImports()
        {
            const string sql = @"
SELECT i.Label, i.Channel, i.Currency, i.FromTimestamp, i.CurrentTimestamp, i.Enabled
FROM Import i;";
            var queryResult = DbContext.QueryTransaction<Db.Import>(sql);

            return queryResult.Select(ToImport).ToList();
        }

        private static Import ToImport(Db.Import import)
        {
            return new Import(import.Label, import.Channel, (Unit)import.Currency, import.FromTimestamp, import.CurrentTimestamp, import.Enabled);
        }

        public void AddImport(Import import)
        {
            ArgumentNullException.ThrowIfNull(import);

            using var transaction = DbContext.BeginTransaction();
            try
            {
                var labelObisCodes = DbContext.Connection.Query<long>("SELECT oc.ObisCode FROM LabelObisLive lol JOIN Label lbl ON lol.LabelId=lbl.Id JOIN Obis oc ON lol.ObisId=oc.Id WHERE lbl.LabelName=@Label;", import, transaction).Select(x => (ObisCode)x).ToList();
                if (labelObisCodes.Any(x => x != ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat))
                {
                    throw new DataStoreUniqueConstraintException($"Label '{import.Label}' already alotted for other and non compatible obis codes:{string.Join(", ", labelObisCodes)}");
                }

                var dbImport = new Db.Import
                {
                    Label = import.Label,
                    Channel = import.Channel,
                    Currency = (int)import.Currency,
                    FromTimestamp = import.FromTimestamp,
                    CurrentTimestamp = import.CurrentTimestamp,
                    Enabled = import.Enabled
                };
                const string sql = "INSERT INTO Import (Label,Channel,Currency,FromTimestamp,CurrentTimestamp,Enabled) VALUES (@Label,@Channel,@Currency,@FromTimestamp,@CurrentTimestamp,@Enabled);";
                DbContext.Connection.Execute(sql, dbImport, transaction);

                transaction.Commit();
            }
            catch (SqliteException e)
            {
                transaction.Rollback();
                throw DataStoreExceptionFactory.Create(e);
            }

        }

        public void DeleteImport(string label)
        {
            ArgumentNullException.ThrowIfNull(label);

            const string sql = "DELETE FROM Import WHERE Label=@label;";
            DbContext.ExecuteTransaction(sql, new { label });
        }

        public void SetEnabled(string label, bool enabled)
        {
            ArgumentNullException.ThrowIfNull(label);

            const string sql = "UPDATE Import SET Enabled=@enabled WHERE Label=@label;";
            DbContext.ExecuteTransaction(sql, new { label, enabled });
        }

        public void SetCurrentTimestamp(string label, DateTime currentTimestamp)
        {
            ArgumentNullException.ThrowIfNull(label);
            ArgCheck.ThrowIfNotUtc(currentTimestamp);

            const string sql = "UPDATE Import SET CurrentTimestamp=@currentTimestamp WHERE Label=@label;";
            DbContext.ExecuteTransaction(sql, new { label, currentTimestamp = (UnixTime)currentTimestamp });
        }

    }
}
