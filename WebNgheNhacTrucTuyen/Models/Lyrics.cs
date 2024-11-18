using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebNgheNhacTrucTuyen.Models
{
    public class Lyrics
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int L_Id { get; set; }
        public string FilePath { get; set; } // Đường dẫn file lyrics
        public int SongId { get; set; }
        public Songs Song { get; set; } // Quan hệ 1-1 với bài hát
    }
}
