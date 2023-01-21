using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PowerView.Model;

namespace PowerView.Service.Dtos
{
    public class LabelRegisterValueDto
    {
        [Required]
        [MinLength(1)]
        public string Label { get; set; }

        [Required]
        [UtcDateTime]
        public DateTime? Timestamp { get; set; }

        [Required]
        [ObisCode]
        public string ObisCode { get; set; }

        [Required]
        public int? Value { get; set; }

        [Required]
        public short? Scale { get; set; }

        [Required]
        public string Unit { get; set; }

        [Required]
        [MinLength(1)]
        public string DeviceId { get; set; }
    }
}
