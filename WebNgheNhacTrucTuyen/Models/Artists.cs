using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebNgheNhacTrucTuyen.Models
{
    public class Artists
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ART_Id { get; set; }
        public string ART_Name { get; set; } 
        public string? ART_Description { get; set; } 
        public string ART_Image { get; set; } = "default-artist.png";


        // Danh sách bài hát của nghệ sĩ
        public virtual ICollection<Songs> Songs { get; set; } = new List<Songs>();

        public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
    }
}
