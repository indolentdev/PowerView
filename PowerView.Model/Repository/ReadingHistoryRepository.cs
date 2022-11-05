using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PowerView.Model.Repository
{
    internal class ReadingHistoryRepository : RepositoryBase, IReadingHistoryRepository
    {
        public ReadingHistoryRepository(IDbContext dbContext)
          : base(dbContext)
        {
        }

        public void ClearDayMonthYearHistory()
        {
            DbContext.ExecuteTransaction(@"
DELETE FROM YearRegister;
DELETE FROM YearReading;
DELETE FROM MonthRegister;
DELETE FROM MonthReading;            
DELETE FROM DayRegister;
DELETE FROM DayReading;
DELETE FROM StreamPosition;");
        }

    }
}
