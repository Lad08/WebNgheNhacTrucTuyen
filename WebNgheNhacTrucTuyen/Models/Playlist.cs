using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebNgheNhacTrucTuyen.Models
{
    public class Playlist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int P_Id { get; set; }
        public string P_Name { get; set; }

        public string P_Image { get; set; }

        public bool IsFavoritePlaylist { get; set; } = false;

        public string UserId { get; set; } // Người tạo playlist
        public virtual Users User { get; set; } // Navigation property

        // Danh sách bài hát trong playlist
        public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();


    }

    public class PlaylistSong
    {
        [Key]
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        [ForeignKey("PlaylistId")]
        public virtual Playlist Playlist { get; set; }

        public int SongId { get; set; }
        [ForeignKey("SongId")]
        public virtual Songs Song { get; set; }
    }

}
