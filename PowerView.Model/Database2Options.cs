using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace PowerView.Model
{
    public class Database2Options : IOptions<Database2Options>
    {
        [MinLength(1)]
        public string TimeZone { get; set;}

        [MinLength(1)]
        public string CultureInfo { get; set; }

        Database2Options IOptions<Database2Options>.Value => this;
    }

}
