using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebNgheNhacTrucTuyen.Data;

namespace WebNgheNhacTrucTuyen.Models
{
    public class Songs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string CoverImagePath { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        public bool IsFavorite { get; set; }

        // Foreign key đến User
        public string UserId { get; set; } // Thêm thuộc tính UserId
        public virtual Users User { get; set; } // Thêm navigation property

        // Foreign key đến Genre
        public int GenreId { get; set; } // Thêm thuộc tính GenreId
        [ForeignKey("GenreId")] // Đánh dấu thuộc tính GenreId là khóa ngoại
        public virtual Genres Genre { get; set; } // Thêm navigation property đến Genres

       

    }
}
