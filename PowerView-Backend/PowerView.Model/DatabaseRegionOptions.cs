using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace PowerView.Model
{
    public class DatabaseRegionOptions : IOptions<DatabaseRegionOptions>
    {
        [MinLength(1)]
        public string TimeZone { get; set; }

        [MinLength(1)]
        public string CultureInfo { get; set; }

        DatabaseRegionOptions IOptions<DatabaseRegionOptions>.Value => this;
    }

}
