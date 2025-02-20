using System.ComponentModel.DataAnnotations;

namespace PowerView.Service.Dtos
{
    public class LiveReadingSetDto
    {
        [Required]
        public LiveReadingDto[] Items { get; set; }
    }
}

