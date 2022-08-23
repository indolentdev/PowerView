using System.ComponentModel.DataAnnotations;

namespace PowerView.Service.Dtos
{
    public class LiveReadingDto : IValidatableObject
    {
        [Required]
        public string Label { get; set; }

        public string DeviceId { get; set; }

        public int? SerialNumber { get; set; }

        [Required]
        [UtcDateTime]
        public DateTime? Timestamp { get; set; }

        [Required]
        public RegisterValueDto[] RegisterValues { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(DeviceId) && SerialNumber == null)
            {
                yield return new ValidationResult("DeviceId or SerialNumber must be present. Its recommended to use DeviceId. SerialNumber is obsolete.", new[] { nameof(DeviceId), nameof(SerialNumber) });
            }
       }
    }
}