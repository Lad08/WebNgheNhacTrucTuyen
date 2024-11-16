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
            return View("Upload");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, string title, string artist, string Genre)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Chọn một file để upload.");
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

            // Lưu thông tin bài hát vào database
            var song = new Songs
            {
                Title = title,
                Artist = artist,
                Genre = Genre,
                FilePath = "/music/" + file.FileName,
                UploadDate = DateTime.Now
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

        public IActionResult Details(int id)
        {
            var song = _context.Songs.Find(id);
            if (song == null)
            {
                return NotFound();
            }
            return View(song);
        }
    }
}
