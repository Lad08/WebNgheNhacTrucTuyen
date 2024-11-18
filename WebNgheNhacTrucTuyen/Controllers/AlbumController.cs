using Microsoft.AspNetCore.Mvc;
using WebNgheNhacTrucTuyen.Data;

namespace WebNgheNhacTrucTuyen.Controllers
{
    public class AlbumController : Controller
    {
        private readonly AppDBContext _context;

        public AlbumController(AppDBContext context)
        {
            _context = context;
        }
        public IActionResult Create()
        {
            return View();
        }
    }
}
