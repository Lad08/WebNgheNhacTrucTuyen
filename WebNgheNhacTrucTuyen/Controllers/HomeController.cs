using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebNgheNhacTrucTuyen.Data;
using WebNgheNhacTrucTuyen.Models;

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

            // Lấy tất cả bài hát và nhóm theo thể loại
            var songsByGenre = await _context.Songs
                .Include(s => s.Genre) // Bao gồm thông tin thể loại
                .Include(s => s.Artist)
                .ToListAsync();

            return View(songsByGenre);

        }

        public async Task<IActionResult> SongDetails(int id)
        {
            var song = await _context.Songs
                               .Include(s => s.User) // Bao gồm User
                               .Include(s => s.Genre) // Bao gồm thể loại
                               .Include(s => s.Lyrics) // Bao gồm lyrics
                               .Include(s => s.Artist)
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
