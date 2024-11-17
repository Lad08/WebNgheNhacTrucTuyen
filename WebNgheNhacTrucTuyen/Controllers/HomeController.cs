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
        public HomeController(ILogger<HomeController> logger, AppDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy 5 bài hát mới nhất
            var latestSongs = await _context.Songs.OrderByDescending(s => s.UploadDate).Take(5).ToListAsync();

            // Truyền dữ liệu vào view
            return View(latestSongs);
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
