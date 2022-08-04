using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PowerView.Model;

namespace PowerView.Service.Dtos
{
    public class RegisterValueDto
    {
        [Required]
        [ObisCode]
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
