using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PowerView.Model;

namespace PowerView.Service.Dtos
{
    public class SerieColorDto
    {
        [Required]
        public string Label { get; set; }

        [Required]
        [ObisCode]
        public string ObisCode { get; set; }

        [Required]
        public string Color { get; set; }
    }
}
