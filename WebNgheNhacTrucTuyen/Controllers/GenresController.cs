using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebNgheNhacTrucTuyen.Data;
using WebNgheNhacTrucTuyen.Models;

namespace WebNgheNhacTrucTuyen.Controllers
{
    public class GenresController : Controller
    {

        private readonly AppDBContext _context;

        public GenresController(AppDBContext context)
        {
            _context = context;
        }

        // GET: Genres/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Genres/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Genres genre)
        {
            if (ModelState.IsValid)
            {
                _context.Genres.Add(genre);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Chuyển hướng đến trang danh sách thể loại
            }
            return View(genre);
        }

        // GET: Genres
        public IActionResult Index()
        {
            var genres = _context.Genres.ToList();
            return View(genres);
        }

    }
}
