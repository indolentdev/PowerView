using System.ComponentModel.DataAnnotations;
using PowerView.Model;

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

            var duplicateObisCodes = RegisterValues
              .Where(x => x != null && x.ObisCode != null)
              .GroupBy(x => x.ObisCode)
              .Select(x => new { ObisCode = x.Key, Registers = x.ToList() })
              .Where(x => x.Registers.Count > 1)
              .ToList();

            if (duplicateObisCodes.Count > 0)
            {
                var duplicateObisCodesString = string.Join(", ", duplicateObisCodes.Select(x => x.ObisCode));
                yield return new ValidationResult($"ObisCodes must be unique within a label and timestamp. Label:{Label}, Timestamp:{Timestamp?.ToString("o")}, Duplicate ObisCodes:{duplicateObisCodesString}", new[] { nameof(RegisterValues) });
            }
        }
    }
}