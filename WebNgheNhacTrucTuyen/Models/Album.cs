using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebNgheNhacTrucTuyen.Models
{
    public class Album
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int A_Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string CoverImagePath { get; set; }

        public DateTime ReleaseDate { get; set; }

        // Foreign key đến User
        public string UserId { get; set; }
        public virtual Users User { get; set; }

        // Danh sách các bài hát trong album
        public virtual ICollection<Songs> Songs { get; set; }
    }
}
