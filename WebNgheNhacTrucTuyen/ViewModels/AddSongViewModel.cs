using WebNgheNhacTrucTuyen.Models;

namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class AddSongViewModel
    {
        public int AlbumId { get; set; }
        public IEnumerable<Songs> Songs { get; set; }
    }
}
