using System.ComponentModel.DataAnnotations;
namespace PowerView.Service.EnergiDataService;

/// <summary>
/// https://www.energidataservice.dk/tso-electricity/DayAheadPrices
/// </summary>
public class DayAheadPriceRecordDto
{
    [Required]
    public DateTime? TimeUtc { get; set; }

    /// <summary>
    /// DKK per MWH
    /// </summary>
    [Required]
    public double? DayAheadPriceDkk { get; set; }

    /// <summary>
    /// EUR per MWH
    /// </summary>
    [Required]
    public double? DayAheadPriceEur { get; set; }

    public KwhAmount GetKwhAmount()
    {
        return new KwhAmount
        {
            Start = DateTime.SpecifyKind(TimeUtc.Value, DateTimeKind.Utc),
            Duration = TimeSpan.FromMinutes(15),
            AmountDkk = DayAheadPriceDkk.Value / 1000,
            AmountEur = DayAheadPriceEur.Value / 1000
        };
    }
}