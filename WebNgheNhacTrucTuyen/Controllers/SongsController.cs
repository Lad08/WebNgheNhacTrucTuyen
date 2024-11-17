using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebNgheNhacTrucTuyen.Models;
using WebNgheNhacTrucTuyen.Data;


namespace WebNgheNhacTrucTuyen.Controllers
{
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

        public async Task<IActionResult> Library()
        {
            // Lấy 5 bài hát mới nhất
            var latestSongs = await _context.Songs.OrderByDescending(s => s.UploadDate).Take(5).ToListAsync();
            return View(latestSongs);
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
        .Include(s => s.User) // Bao gồm thông tin người dùng
        .FirstOrDefaultAsync(s => s.Id == id);
            if (song == null)
            {
                return NotFound();
            }
            return View(song);
        }
    }
}
