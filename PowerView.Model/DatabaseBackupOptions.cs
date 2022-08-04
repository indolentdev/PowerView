using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace PowerView.Model
{
    public class DatabaseBackupOptions : IOptions<DatabaseBackupOptions>
    {
        [Range(typeof(TimeSpan), "14.00:00:00", "10675199.00:00:00")]
        public TimeSpan MinimumInterval { get; set;} = TimeSpan.FromDays(14);

        [Range(1, 10)]
        public int MaximumCount { get; set; } = 5;

        DatabaseBackupOptions IOptions<DatabaseBackupOptions>.Value => this;
    }

}
