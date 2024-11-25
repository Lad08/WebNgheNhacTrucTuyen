using WebNgheNhacTrucTuyen.Models;

namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Songs> LatestSongs { get; set; } // Danh sách bài hát mới nhất
        public IEnumerable<Songs> SongsByEDM { get; set; }  // Danh sách bài hát thể loại EDM
        public IEnumerable<Songs> SongsByBGM { get; set; }  // Danh sách bài hát thể loại BGM
        public IEnumerable<Playlist> Playlists { get; set; } // Danh sách playlist
        public IEnumerable<Album> Albums { get; set; }       // Danh sách album

        public List<Artists> Artists { get; set; } = new List<Artists>(); // Danh sách Nghệ sĩ
    }
}
