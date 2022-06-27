using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PowerView.Model;

namespace PowerView.Service.Dtos
{
    public class RegisterValueDto
    {
        [Required]
        [RegularExpression("^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){5}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", ErrorMessage = "Invalid ObisCode.")]
        public string ObisCode { get; set; }

        [Required]
        public int? Value { get; set; }

        [Required]
        public short? Scale { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public Unit? Unit { get; set; }
    }
}
