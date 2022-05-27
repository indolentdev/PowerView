using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;


namespace PowerView.Configuration
{
    public class ServiceOptions : IOptions<ServiceOptions>
    {
        [Required]
        public string BaseUrl { get; set; } = string.Empty;

        ServiceOptions IOptions<ServiceOptions>.Value => this;
    }

}
