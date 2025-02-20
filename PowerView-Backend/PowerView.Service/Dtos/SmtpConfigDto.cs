using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PowerView.Model;

namespace PowerView.Service.Dtos
{
    public class SmtpConfigDto
    {
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string Server { get; set; }

        [Required]
        public ushort? Port { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string User { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string Auth { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
