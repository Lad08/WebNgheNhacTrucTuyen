using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebNgheNhacTrucTuyen.Models
{
    public class Artists
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; } // Tên nghệ sĩ
        


        // Danh sách bài hát của nghệ sĩ
        public virtual ICollection<Songs> Songs { get; set; } = new List<Songs>();
    }
}
