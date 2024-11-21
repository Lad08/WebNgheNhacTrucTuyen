using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class AlbumCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        // File ảnh bìa (có thể null)
        public IFormFile? CoverImage { get; set; }
    }
}
