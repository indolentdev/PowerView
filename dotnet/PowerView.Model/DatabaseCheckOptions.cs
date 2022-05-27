using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace PowerView.Model
{
    public class DatabaseCheckOptions : IOptions<DatabaseCheckOptions>
    {
        public ushort IntegrityCheckCommandTimeout { get; set; } = 600;

        DatabaseCheckOptions IOptions<DatabaseCheckOptions>.Value => this;
    }

}
