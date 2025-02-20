using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PowerView.Model;

namespace PowerView.Service.Dtos
{
    public class CostBreakdownDto
    {
        [Required]
        [StringLength(30, MinimumLength = 1)]
        public string Title { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public Unit? Currency { get; set; }

        [Required]
        [Range(1, 100)]
        public int? Vat { get; set; }
    }
}
