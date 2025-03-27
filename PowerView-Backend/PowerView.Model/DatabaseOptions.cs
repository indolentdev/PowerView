using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace PowerView.Model
{
    public class DatabaseOptions : IOptions<DatabaseOptions>
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; } = string.Empty;

        internal bool OptimizeOnClose {get; set; } = true;

        DatabaseOptions IOptions<DatabaseOptions>.Value => this;
    }

}
