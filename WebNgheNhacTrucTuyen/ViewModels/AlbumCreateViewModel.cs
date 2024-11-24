using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public int? ArtistId { get; set; } // ID của nghệ sĩ (nếu chọn nghệ sĩ)

        

        // Danh sách nghệ sĩ để hiển thị trong dropdown
        public List<SelectListItem> Artists { get; set; } = new List<SelectListItem>();
    }
}
