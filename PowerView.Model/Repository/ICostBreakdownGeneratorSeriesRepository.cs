﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Dapper;

namespace PowerView.Model.Repository;

public interface ICostBreakdownGeneratorSeriesRepository
{
  ICollection<CostBreakdownGeneratorSeries> GetCostBreakdownGeneratorSeries();
}
