using System.ComponentModel.DataAnnotations;

namespace PowerView.Service.Dtos
{
    public class LiveReadingDto : IValidatableObject
    {
        [Required]
        public string Label { get; set; }

        [Required]
        public string DeviceId { get; set; }

        [Required]
        public DateTime? Timestamp { get; set; }

        [Required]
        public RegisterValueDto[] RegisterValues { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Timestamp.Value.Kind != DateTimeKind.Utc)
            {
                yield return new ValidationResult($"Timestamp must be UTC. Was:{Timestamp!.Value.Kind}", new[] { nameof(Timestamp) });
            }
        }
    }
}