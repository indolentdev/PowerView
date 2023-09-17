using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Dapper;

namespace PowerView.Model.Repository;

public interface ICostBreakdownRepository
{
  ICollection<string> GetCostBreakdownTitles();

  CostBreakdown GetCostBreakdown(string title);

  ICollection<CostBreakdown> GetCostBreakdowns();

  void AddCostBreakdown(CostBreakdown costBreakdown);

  void DeleteCostBreakdown(string title);

  void AddCostBreakdownEntry(string title, CostBreakdownEntry costBreakdownEntry);

  void UpdateCostBreakdownEntry(string title, DateTime fromDate, DateTime toDate, string name, CostBreakdownEntry costBreakdownEntry);

  void DeleteCostBreakdownEntry(string title, DateTime fromDate, DateTime toDate, string name);
}
