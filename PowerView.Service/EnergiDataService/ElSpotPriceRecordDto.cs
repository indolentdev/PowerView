using System.ComponentModel.DataAnnotations;
namespace PowerView.Service.EnergiDataService;

/// <summary>
/// https://www.energidataservice.dk/tso-electricity/Elspotprices
/// </summary>
public class ElSpotPriceRecordDto
{
    [Required]
    public DateTime? HourUtc { get; set; }

    /// <summary>
    /// DKK per MWH
    /// </summary>
    [Required]
    public double? SpotPriceDkk { get; set; }

    /// <summary>
    /// EUR per MWH
    /// </summary>
    [Required]
    public double? SpotPriceEur { get; set; }

    public KwhAmount GetKwhAmount()
    {
        return new KwhAmount
        {
            Start = DateTime.SpecifyKind(HourUtc.Value, DateTimeKind.Utc),
            Duration = TimeSpan.FromHours(1),
            AmountDkk = SpotPriceDkk.Value / 1000,
            AmountEur = SpotPriceEur.Value / 1000

    };
    }
}