using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PowerView.Model;

namespace PowerView.Service.Dtos
{
    public class ProfileGraphDto : IValidatableObject
    {
        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string Period { get; set; }

        [StringLength(32, MinimumLength = 0)]
        public string Page { get; set; }

        [Required]
        [StringLength(32, MinimumLength = 1)]
        public string Title { get; set; }

        [Required]
        [StringLength(32, MinimumLength = 1)]
        public string Interval { get; set; }

        [Required]
        [MinLength(1)]
        public ProfileGraphSerieDto[] Series { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Period != "day" && Period != "month" && Period != "year")
            {
                yield return new ValidationResult("Value invalid. Must be day, month or year", new[] { nameof(Period) });
            }

            var distinctSeriesCount = Series.Select(x => new { x.Label, x.ObisCode }).Distinct().Count();
            if (Series.Length != distinctSeriesCount)
            {
                yield return new ValidationResult("Series must be distinct", new[] { nameof(Series) });
            }
        }
    }
}
