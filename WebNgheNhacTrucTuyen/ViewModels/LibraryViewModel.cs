using WebNgheNhacTrucTuyen.Models;

namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class LibraryViewModel
    {
        public List<Songs> FavoriteSongs { get; set; }
        public List<Songs> UploadedSongs { get; set; }
        public List<Album> UserAlbums { get; set; }
    }
}
