using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PowerView.Model;

namespace PowerView.Service.Dtos
{
    public class GeneratorSeriesDto
    {
        [Required]
        [StringLength(25, MinimumLength = 1)]
        public string NameLabel { get; set; }

        [Required]
        [ObisCode]
        public string NameObisCode { get; set; }

        [Required]
        [StringLength(25, MinimumLength = 1)]
        public string BaseLabel { get; set; }

        [Required]
        [ObisCode]
        public string BaseObisCode { get; set; }

        [Required]
        [StringLength(25, MinimumLength = 1)]
        public string CostBreakdownTitle { get; set; }
    }
}
