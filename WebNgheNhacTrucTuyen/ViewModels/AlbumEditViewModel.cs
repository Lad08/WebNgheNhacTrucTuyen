using System.ComponentModel.DataAnnotations;

namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class AlbumEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên album không được để trống.")]
        public string Name { get; set; }

        public string? Description { get; set; }

        // Ảnh bìa hiện tại
        public string? ExistingCoverImagePath { get; set; }

        // Ảnh bìa mới (nếu có)
        [DataType(DataType.Upload)]
        public IFormFile? CoverImage { get; set; }
    }
}
