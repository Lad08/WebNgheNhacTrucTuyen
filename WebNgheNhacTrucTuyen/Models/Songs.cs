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
        public string Genre { get; set; }

        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }

    }
}
