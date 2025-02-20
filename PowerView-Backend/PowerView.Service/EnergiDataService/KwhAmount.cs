
namespace PowerView.Service.EnergiDataService;

/// <summary>
/// Expense or income amount for one kWh of electricity energy excl. VAT, tariff or other carges
/// </summary>
public class KwhAmount
{
    public DateTime Start { get; set; }
    public TimeSpan Duration { get; set; }
    public double AmountDkk { get; set; }
    public double AmountEur { get; set; }
}