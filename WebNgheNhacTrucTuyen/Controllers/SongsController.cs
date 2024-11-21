using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebNgheNhacTrucTuyen.Models;
using WebNgheNhacTrucTuyen.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebNgheNhacTrucTuyen.ViewModels;


namespace WebNgheNhacTrucTuyen.Controllers
{
    [Authorize(Roles = "User,Admin ")]
    public class SongsController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<SongsController> _logger;

        public SongsController(AppDBContext context, UserManager<Users> userManager, IWebHostEnvironment environment, ILogger<SongsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            ViewBag.Genres = _context.Genres.ToList(); // Lấy danh sách thể loại
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, IFormFile coverImage, string title, string artistName, int genreId)
        {
            if (file == null || file.Length == 0 || coverImage == null || coverImage.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file nhạc và ảnh bìa hợp lệ.");
                return View();
            }

            // Lấy thông tin người dùng đăng nhập
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Kiểm tra và thêm nghệ sĩ nếu chưa tồn tại
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.Name == artistName);
            if (artist == null)
            {
                artist = new Artists { Name = artistName };
                _context.Artists.Add(artist);
                await _context.SaveChangesAsync(); // Lưu vào DB để lấy Id
            }

            // Tạo thư mục lưu file nhạc nếu chưa tồn tại
            var filePath = Path.Combine(_environment.WebRootPath, "music", file.FileName);
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            // Lưu file nhạc
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Tạo thư mục lưu ảnh bìa nếu chưa tồn tại
            var coverImagePath = Path.Combine(_environment.WebRootPath, "images/song_img", coverImage.FileName);
            if (!Directory.Exists(Path.GetDirectoryName(coverImagePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(coverImagePath));
            }

            // Lưu ảnh bìa
            using (var stream = new FileStream(coverImagePath, FileMode.Create))
            {
                await coverImage.CopyToAsync(stream);
            }

            // Lưu thông tin bài hát vào cơ sở dữ liệu
            var song = new Songs
            {
                Title = title,
                FilePath = "/music/" + file.FileName,
                CoverImagePath = "/images/song_img/" + coverImage.FileName,
                UserId = user.Id,
                UploadDate = DateTime.Now,
                GenreId = genreId,
                ArtistId = artist.Id // Liên kết với nghệ sĩ
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Bài hát đã được tải lên thành công.";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Library(string genre)
        {
            // Lấy tất cả thể loại
            var genres = await _context.Genres.ToListAsync();
            ViewBag.Genres = genres;

            // Tạo truy vấn bài hát
            IQueryable<Songs> songsQuery = _context.Songs
                .Include(s => s.Genre)
                .Include(s => s.Artist); // Bao gồm thông tin nghệ sĩ (nếu cần)

            // Nếu genre không null, lọc theo thể loại
            if (!string.IsNullOrEmpty(genre))
            {
                songsQuery = songsQuery.Where(s => s.Genre.G_Name == genre);
            }

            // Lấy danh sách bài hát từ truy vấn
            var songs = await songsQuery.ToListAsync();

            // Lọc bài hát yêu thích
            var favoriteSongs = songs.Where(s => s.IsFavorite).ToList();

            return View(favoriteSongs);
        }

        public async Task<IActionResult> Play(int id)
        {
            // Tìm bài hát theo Id
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
            {
                return NotFound("Bài hát không tồn tại.");
            }

            // Kiểm tra đường dẫn file nhạc
            var filePath = Path.Combine(_environment.WebRootPath, song.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File nhạc không tồn tại.");
            }

            // Trả về file nhạc dưới dạng FileStreamResult để phát nhạc
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(stream, "audio/mpeg", enableRangeProcessing: true);
        }

        public async Task<IActionResult> Details(int id)
        {
            var song = await _context.Songs
                    .Include(s => s.User) // Bao gồm User
                    .Include(s => s.Genre) // Bao gồm thể loại
                    .Include(s => s.Lyrics) // Bao gồm lyrics
                    .FirstOrDefaultAsync(s => s.Id == id);

            if (song == null)
            {
                return NotFound("Bài hát không tồn tại.");
            }

            // Đọc nội dung file lyrics nếu tồn tại
            if (song.Lyrics != null && !string.IsNullOrEmpty(song.Lyrics.FilePath))
            {
                string fullPath = Path.Combine(_environment.WebRootPath, song.Lyrics.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    ViewBag.LyricsContent = System.IO.File.ReadAllText(fullPath);
                }
            }

            return View(song);
        }

        // Chức năng bài hát yêu thích
        public IActionResult ToggleFavorite(int id)
        {
            var song = _context.Songs.Find(id);
            if (song != null)
            {
                song.IsFavorite = !song.IsFavorite;
                _context.SaveChanges();
            }
            return RedirectToAction("Library");
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
            var song = await _context.Songs.Include(s => s.Lyrics).FirstOrDefaultAsync(s => s.Id == id);
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
                song.Lyrics.FilePath = $"/Lyrics/{fileName}";
                _context.Lyrics.Update(song.Lyrics);
            }
            else
            {
                // Nếu chưa có lyrics, thêm mới
                var lyrics = new Lyrics
                {
                    FilePath = $"/Lyrics/{fileName}",
                    SongId = song.Id
                };
                _context.Lyrics.Add(lyrics);
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Lyrics đã được tải lên thành công.";
            return RedirectToAction("Details", new { id });
        }


    }
}
