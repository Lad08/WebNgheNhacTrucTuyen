using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebNgheNhacTrucTuyen.Data;

namespace WebNgheNhacTrucTuyen.Models
{
    public class Songs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int S_Id { get; set; }
        public string S_Title { get; set; }
        public string S_CoverImagePath { get; set; }
        public string S_FilePath { get; set; }
        public DateTime S_UploadDate { get; set; }
        public bool S_IsFavorite { get; set; }

        public string? S_Description { get; set; }

        // Foreign key đến Artist
        public int ArtistId { get; set; } // Khóa ngoại
        [ForeignKey("ArtistId")]
        public virtual Artists Artist { get; set; }

        // Đường dẫn tới file lyrics được lưu trên server
        public Lyrics Lyrics { get; set; }

        // Foreign key đến User
        public string UserId { get; set; } // Thêm thuộc tính UserId
        public virtual Users User { get; set; } // Thêm navigation property

        // Foreign key đến Genre
        public int GenreId { get; set; } // Thêm thuộc tính GenreId
        [ForeignKey("GenreId")] // Đánh dấu thuộc tính GenreId là khóa ngoại
        public virtual Genres Genre { get; set; } // Thêm navigation property đến Genres

        // Foreign key đến Album
        public int? AlbumId { get; set; } // AlbumId có thể null nếu bài hát không thuộc album nào
        [ForeignKey("AlbumId")]
        public virtual Album Album { get; set; }

        public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; }

    }
}
