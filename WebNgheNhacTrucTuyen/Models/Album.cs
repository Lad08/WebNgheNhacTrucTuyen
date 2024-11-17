using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebNgheNhacTrucTuyen.Models
{
    public class Album
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int A_Id { get; set; }
        public string Name { get; set; }
        public string Mota { get; set; }
    }
}
