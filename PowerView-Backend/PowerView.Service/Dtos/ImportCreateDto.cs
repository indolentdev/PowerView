using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PowerView.Model;

namespace PowerView.Service.Dtos
{
    public class ImportCreateDto
    {
        [Required]
        [StringLength(25, MinimumLength = 1)]
        public string Label { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Channel { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public Unit? Currency { get; set; }

        [Required]
        [UtcDateTime]
        public DateTime? FromTimestamp { get; set; }
    }
}
