using System.ComponentModel.DataAnnotations;
namespace PowerView.Service.EnergiDataService;

public class DayAheadPricesDto
{
    [Required]
    public List<DayAheadPriceRecordDto> Records { get; set; }
}