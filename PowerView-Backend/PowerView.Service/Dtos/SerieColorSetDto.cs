using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PowerView.Model;

namespace PowerView.Service.Dtos
{
    public class SerieColorSetDto
    {
        [Required]
        public SerieColorDto[] Items { get; set; }
    }
}
