using System.ComponentModel.DataAnnotations;

namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class UploadLyricsViewModel
    {
        public int SongId { get; set; }

        [Display(Name = "File Lyrics (.txt)")]
        public IFormFile? File { get; set; }

        [Display(Name = "Nội dung lyrics")]
        public string? Content { get; set; }

        public string? ExistingFilePath { get; set; }
    }
}
