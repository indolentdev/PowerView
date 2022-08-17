using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PowerView.Model;

namespace PowerView.Service.Dtos
{
    public class MqttConfigDto
    {
        [Required]
        public bool? PublishEnabled { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string Server { get; set; }

        [Required]
        public ushort? Port { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string ClientId { get; set; }
    }
}
