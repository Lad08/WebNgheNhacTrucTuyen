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
        public string A_Name { get; set; }

        public string A_Description { get; set; }
        
        public string? A_CoverImagePath { get; set; }

        public DateTime A_CreatedDate { get; set; }

        // Liên kết với bảng Songs
        public virtual ICollection<Songs> Songs { get; set; } = new List<Songs>();
        public string? UserId { get; set; }
    }
}
