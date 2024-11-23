using WebNgheNhacTrucTuyen.Models;

namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class AddSongToPlaylistViewModel
    {
        public int PlaylistId { get; set; }
        public int SongId { get; set; }
        public List<Playlist> Playlists { get; set; }
        public List<Songs> Songs { get; set; }
    }
}
