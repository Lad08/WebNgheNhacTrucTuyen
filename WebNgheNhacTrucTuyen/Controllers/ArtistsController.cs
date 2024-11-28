using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebNgheNhacTrucTuyen.Data;
using WebNgheNhacTrucTuyen.Models;

namespace WebNgheNhacTrucTuyen.Controllers
{
    public class ArtistsController : Controller
    {
        private readonly AppDBContext _context;

        public ArtistsController(AppDBContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách artists
        public async Task<IActionResult> Index()
        {
            var artists = await _context.Artists.ToListAsync();
            return View(artists);
        }

        // GET: Tạo mới artist
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Artists artist, IFormFile? ART_ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ART_ImageFile != null && ART_ImageFile.Length > 0)
                {
                    // Lưu ảnh vào thư mục wwwroot/images/artists
                    var fileName = Path.GetFileName(ART_ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/artists", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ART_ImageFile.CopyToAsync(stream);
                    }

                    artist.ART_Image = fileName;
                }

                _context.Add(artist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(artist);
        }

        // GET: Sửa artist
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artist = await _context.Artists.FindAsync(id);
            if (artist == null)
            {
                return NotFound();
            }
            return View(artist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Artists artist, IFormFile? ART_ImageFile)
        {
            if (id != artist.ART_Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ART_ImageFile != null && ART_ImageFile.Length > 0)
                    {
                        // Lưu ảnh mới
                        var fileName = Path.GetFileName(ART_ImageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/artists", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ART_ImageFile.CopyToAsync(stream);
                        }

                        // Cập nhật đường dẫn ảnh
                        artist.ART_Image = fileName;
                    }

                    _context.Update(artist);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArtistExists(artist.ART_Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(artist);
        }


        // GET: Xóa artist
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artist = await _context.Artists
                .FirstOrDefaultAsync(m => m.ART_Id == id);
            if (artist == null)
            {
                return NotFound();
            }

            return View(artist);
        }

        // POST: Xóa artist
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist != null)
            {
                _context.Artists.Remove(artist);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ArtistExists(int id)
        {
            return _context.Artists.Any(e => e.ART_Id == id);
        }

        // GET: Chi tiết nghệ sĩ
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artist = await _context.Artists
                .Include(a => a.Songs) // Bao gồm danh sách bài hát
                .Include(a => a.Albums) // Bao gồm danh sách album
                .FirstOrDefaultAsync(a => a.ART_Id == id);

            if (artist == null)
            {
                return NotFound();
            }

            // Lấy 6 bài hát mới upload
            artist.Songs = artist.Songs
                .OrderByDescending(s => s.S_UploadDate) // Giả sử `UploadDate` là thuộc tính ngày upload
                .ToList();

            // Lấy 6 album đầu tiên
            artist.Albums = artist.Albums.ToList();

            return View(artist);
        }


    }
}
