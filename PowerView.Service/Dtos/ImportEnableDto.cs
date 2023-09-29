using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PowerView.Model;

namespace PowerView.Service.Dtos
{
    public class ImportEnableDto
    {
        [Required]
        public bool? Enabled { get; set; }
    }
}
