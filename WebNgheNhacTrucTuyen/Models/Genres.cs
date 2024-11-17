using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebNgheNhacTrucTuyen.Models
{
    public class Genres
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int G_Id { get; set; }

        public string G_Name { get; set; }

        public virtual ICollection<Songs> Songs { get; set; } = new List<Songs>();
    }
}
