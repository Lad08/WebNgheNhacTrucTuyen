using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebNgheNhacTrucTuyen.Models;
using WebNgheNhacTrucTuyen.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;


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
        public async Task<IActionResult> Upload(IFormFile file, IFormFile coverImage, string title, string artist, int genreId)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Chọn một file để upload.");
                return View();
            }

            if (coverImage == null || coverImage.Length == 0)
            {
                ModelState.AddModelError("", "Chọn một ảnh bìa để upload.");
                return View();
            }

            // Lấy UserId của người đang đăng nhập
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Tạo thư mục lưu trữ file
            var filePath = Path.Combine(_environment.WebRootPath, "music", file.FileName);
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            // Lưu file vào thư mục "wwwroot/music"
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Tạo thư mục lưu trữ ảnh bìa
            var coverImagePath = Path.Combine(_environment.WebRootPath, "images/song_img", coverImage.FileName);
            var coverImageDirectory = Path.GetDirectoryName(coverImagePath);
            if (!Directory.Exists(coverImageDirectory)) Directory.CreateDirectory(coverImageDirectory);

            // Lưu ảnh bìa vào thư mục "wwwroot/images/song_img"
            using (var stream = new FileStream(coverImagePath, FileMode.Create))
            {
                await coverImage.CopyToAsync(stream);
            }

            // Lưu thông tin bài hát vào database
            var song = new Songs
            {
                Title = title,
                Artist = artist,
                FilePath = "/music/" + file.FileName,
                CoverImagePath = "/images/song_img/" + coverImage.FileName, // Lưu đường dẫn ảnh bìa
                UserId = user.Id,
                UploadDate = DateTime.Now,
                GenreId = genreId // Lưu GenreId
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home"); // Chuyển hướng về danh sách bài hát
        }

        public async Task<IActionResult> Library(string genre)
        {
            // Lấy tất cả thể loại
            var genres = await _context.Genres.ToListAsync();
            ViewBag.Genres = genres;

            // Tạo truy vấn bài hát
            IQueryable<Songs> songsQuery = _context.Songs.Include(s => s.Genre);

            // Nếu genre không null, lọc theo thể loại
            if (!string.IsNullOrEmpty(genre))
            {
                songsQuery = songsQuery.Where(s => s.Genre.G_Name == genre);
            }

            // Lấy danh sách bài hát yêu thích
            var favoriteSongs = await songsQuery.ToListAsync();

            // Lấy danh sách bài hát yêu thích
            var favoriteSongsList = favoriteSongs.Where(s => s.IsFavorite).ToList();

            // Kết hợp danh sách bài hát yêu thích và bài hát theo thể loại
            var allSongs = favoriteSongsList.Concat(favoriteSongs).Distinct().ToList();

            return View(allSongs);
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
