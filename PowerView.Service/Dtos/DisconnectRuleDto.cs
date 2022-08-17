using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PowerView.Model;

namespace PowerView.Service.Dtos
{
    public class DisconnectRuleDto : IValidatableObject
    {
        [Required]
        public string NameLabel { get; set; }

        [Required]
        [ObisCode]
        public string NameObisCode { get; set; }

        [Required]
        public string EvaluationLabel { get; set; }

        [Required]
        [ObisCode]
        public string EvaluationObisCode { get; set; }

        [Required]
        [Range(15, 6*60)]
        public int? DurationMinutes { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int? DisconnectToConnectValue { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int? ConnectToDisconnectValue { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public DisconnectRuleUnit? Unit { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ConnectToDisconnectValue.Value > DisconnectToConnectValue.Value)
            {
                yield return new ValidationResult("DisconnectToConnectValue must be greater than ConnectToDisconnectValue",
                    new[] { nameof(ConnectToDisconnectValue), nameof(DisconnectToConnectValue) });
            }
        }
    }
}
