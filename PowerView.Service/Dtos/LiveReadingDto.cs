using System.ComponentModel.DataAnnotations;

namespace PowerView.Service.Dtos
{
    public class LiveReadingDto
    {
        [Required]
        public string Label { get; set; }

        [Required]
        public string DeviceId { get; set; }

        [Required]
        [UtcDateTime]
        public DateTime? Timestamp { get; set; }

        [Required]
        public RegisterValueDto[] RegisterValues { get; set; }
    }
}