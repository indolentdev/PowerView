using System.ComponentModel.DataAnnotations;
namespace PowerView.Service.EnergiDataService;

public class ElSpotPricesDto
{
    [Required]
    public List<ElSpotPriceRecordDto> Records { get; set; }
}