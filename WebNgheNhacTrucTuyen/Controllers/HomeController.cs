using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebNgheNhacTrucTuyen.Data;
using WebNgheNhacTrucTuyen.Models;
using WebNgheNhacTrucTuyen.ViewModels;

namespace WebNgheNhacTrucTuyen.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _environment;
        public HomeController(ILogger<HomeController> logger, AppDBContext context, IWebHostEnvironment environment)
        {
            _logger = logger;
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var latestSongs = await _context.Songs
                .OrderByDescending(s => s.S_UploadDate)
                .Take(5)
                .Include(s => s.Genre)
                .Include(s => s.Artist)
                .Include(s => s.Album)
                .ToListAsync();

            var songsByEDM = await _context.Songs
                .Where(s => s.Genre.G_Name == "EDM")
                .Take(5)
                .Include(s => s.Genre)
                .Include(s => s.Artist)
                .Include(s => s.Album)
                .ToListAsync();

            var songsByBGM = await _context.Songs
                .Where(s => s.Genre.G_Name == "BGM")
                .Take(5)
                .Include(s => s.Genre)
                .Include(s => s.Artist)
                .Include(s => s.Album)
                .ToListAsync();

            var playlists = await _context.Playlists
                .Take(5)
                .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                .ToListAsync();

            var albums = await _context.Albums
                .Take(5)
                .Include(a => a.Songs)
                .ToListAsync();

            // Lấy danh sách nghệ sĩ
            var artists = await _context.Artists
                .Take(5)
                .ToListAsync();

            foreach (var artist in artists)
            {
                if (!string.IsNullOrEmpty(artist.ART_Image))
                {
                    artist.ART_Image = $"/{artist.ART_Image.TrimStart('/')}";
                }
            }

            var viewModel = new HomeViewModel
            {
                LatestSongs = latestSongs,
                SongsByEDM = songsByEDM,
                SongsByBGM = songsByBGM,
                Playlists = playlists,
                Albums = albums,
                Artists = artists // Gán dữ liệu nghệ sĩ vào ViewModel
            };

            return View(viewModel);

        }

        public async Task<IActionResult> SongDetails(int id)
        {
            var song = await _context.Songs
                               .Include(s => s.User) // Bao gồm User
                               .Include(s => s.Genre) // Bao gồm thể loại
                               .Include(s => s.Lyrics) // Bao gồm lyrics
                               .Include(s => s.Artist)
                               .Include(s => s.Album)
                               .FirstOrDefaultAsync(s => s.S_Id == id);

            if (song == null)
            {
                return NotFound("Bài hát không tồn tại.");
            }

            // Đọc nội dung file lyrics nếu tồn tại
            if (song.Lyrics != null && !string.IsNullOrEmpty(song.Lyrics.L_FilePath))
            {
                string fullPath = Path.Combine(_environment.WebRootPath, song.Lyrics.L_FilePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    ViewBag.LyricsContent = System.IO.File.ReadAllText(fullPath);
                }
            }

            return View(song);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadLyrics(int id, IFormFile file)
        {
            if (file == null || file.Length == 0 || file.ContentType != "text/plain")
            {
                ModelState.AddModelError("", "Vui lòng tải lên một file .txt hợp lệ.");
                return RedirectToAction("Details", new { id });
            }

            // Tìm bài hát theo Id
            var song = await _context.Songs.Include(s => s.Lyrics).FirstOrDefaultAsync(s => s.S_Id == id);
            if (song == null)
            {
                return NotFound("Bài hát không tồn tại.");
            }

            // Tạo thư mục lưu file lyrics nếu chưa tồn tại
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "Lyrics");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo đường dẫn file lyrics
            string fileName = $"lyrics_{id}.txt";
            string filePath = Path.Combine(uploadsFolder, fileName);

            // Lưu file lyrics
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Nếu bài hát đã có lyrics, cập nhật đường dẫn
            if (song.Lyrics != null)
            {
                song.Lyrics.L_FilePath = $"/Lyrics/{fileName}";
                _context.Lyrics.Update(song.Lyrics);
            }
            else
            {
                // Nếu chưa có lyrics, thêm mới
                var lyrics = new Lyrics
                {
                    L_FilePath = $"/Lyrics/{fileName}",
                    SongId = song.S_Id
                };
                _context.Lyrics.Add(lyrics);
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Lyrics đã được tải lên thành công.";
            return RedirectToAction("Details", new { id });
        }

        public async Task<IActionResult> Search(string searchType, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction("Index");
            }

            if (searchType == "song")
            {
                // Tìm kiếm bài hát
                var songs = await _context.Songs
                    .Where(s => EF.Functions.Like(s.S_Title, $"%{query}%"))
                    .Include(s => s.Artist)
                    .Include(s => s.Genre)
                    .Include(s => s.Album)
                    .ToListAsync();

                return View("Search", new SearchViewModel
                {
                    Query = query,
                    Songs = songs,
                    Artists = null // Không trả về danh sách nghệ sĩ
                });
            }
            else if (searchType == "artist")
            {
                // Tìm kiếm nghệ sĩ
                var artists = await _context.Artists
                    .Where(a => EF.Functions.Like(a.ART_Name, $"%{query}%"))
                    .ToListAsync();

                return View("Search", new SearchViewModel
                {
                    Query = query,
                    Songs = null, // Không trả về danh sách bài hát
                    Artists = artists
                });
            }

            return RedirectToAction("Index");
        }




        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
