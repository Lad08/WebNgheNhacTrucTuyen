using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebNgheNhacTrucTuyen.Data;
using WebNgheNhacTrucTuyen.Models;
using WebNgheNhacTrucTuyen.ViewModels;

namespace WebNgheNhacTrucTuyen.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class PlaylistsController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UserManager<Users> _userManager;

        public PlaylistsController(AppDBContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy thông tin người dùng hiện tại
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Điều hướng đến trang đăng nhập nếu chưa đăng nhập
            }

            // Lấy danh sách playlist của người dùng hiện tại
            var playlists = await _context.Playlists
                .Where(p => p.UserId == user.Id) // Liên kết với user.Id
                .ToListAsync();

            return View(playlists);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string playlistName)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
            {
                TempData["Error"] = "Tên playlist không được để trống.";
                return RedirectToAction(nameof(Index));
            }

            // Lấy thông tin người dùng hiện tại
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Điều hướng nếu chưa đăng nhập
            }

            var playlist = new Playlist
            {
                P_Name = playlistName,
                UserId = user.Id, // Lưu UserId từ UserManager
                
            };

            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Playlist đã được tạo thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSongToPlaylist(int playlistId, int songId)
        {
            // Kiểm tra xem playlist có tồn tại không
            var playlist = await _context.Playlists.Include(p => p.PlaylistSongs).FirstOrDefaultAsync(p => p.P_Id == playlistId);
            if (playlist == null)
            {
                TempData["Error"] = "Playlist không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra xem bài hát có tồn tại không
            var song = await _context.Songs.FirstOrDefaultAsync(s => s.S_Id == songId);
            if (song == null)
            {
                TempData["Error"] = "Bài hát không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra xem bài hát đã có trong playlist chưa
            if (playlist.PlaylistSongs.Any(ps => ps.SongId == songId))
            {
                TempData["Error"] = "Bài hát đã có trong playlist.";
                return RedirectToAction(nameof(Index));
            }

            // Thêm bài hát vào playlist
            var playlistSong = new PlaylistSong
            {
                PlaylistId = playlistId,
                SongId = songId
            };

            _context.PlaylistSongs.Add(playlistSong);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đã thêm bài hát vào playlist.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AddSongToPlaylist()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var playlists = await _context.Playlists
                .Where(p => p.UserId == user.Id)
                .ToListAsync();

            var songs = await _context.Songs.ToListAsync();

            var model = new AddSongToPlaylistViewModel
            {
                Playlists = playlists,
                Songs = songs
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var playlist = await _context.Playlists
                    .Include(p => p.PlaylistSongs)
                        .ThenInclude(ps => ps.Song)
                            .ThenInclude(s => s.Artist) // Include thông tin nghệ sĩ
                    .Include(p => p.PlaylistSongs)
                        .ThenInclude(ps => ps.Song)
                            .ThenInclude(s => s.Genre) // Include thông tin thể loại
                    .FirstOrDefaultAsync(p => p.P_Id == id);

            if (playlist == null)
            {
                return NotFound();
            }

            return View(playlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSongFromPlaylist(int playlistId, int songId)
        {
            var playlistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if (playlistSong == null)
            {
                TempData["Error"] = "Bài hát không tồn tại trong playlist.";
                return RedirectToAction(nameof(Details), new { id = playlistId });
            }

            _context.PlaylistSongs.Remove(playlistSong);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Bài hát đã được xóa khỏi playlist.";
            return RedirectToAction(nameof(Details), new { id = playlistId });
        }

    }
}
