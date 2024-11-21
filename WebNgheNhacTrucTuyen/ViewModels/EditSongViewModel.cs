using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class EditSongViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime UploadDate { get; set; }
        public bool IsFavorite { get; set; }
        public string ArtistName { get; set; } // Nhập nghệ sĩ
        public int GenreId { get; set; }
        public int? AlbumId { get; set; }
    }
}
