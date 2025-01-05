using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PowerView.Model;

namespace PowerView.Service.Dtos
{
    public class CostBreakdownEntryDto : IValidatableObject
    {
        [Required]
        [UtcDateTime]
        public DateTime? FromDate { get; set; }

        [Required]
        [UtcDateTime]
        public DateTime? ToDate { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        [Range(0, 22)]
        public int? StartTime { get; set; }

        [Required]
        [Range(1, 23)]
        public int? EndTime { get; set; }

        [Required]
        [Range(0d, 1000d)]
        public double? Amount { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime <= StartTime)
            {
                yield return new ValidationResult("EndTime Invalid. Must be greater than StartTime", new[] { nameof(EndTime) });
            }
        }
    }
}
