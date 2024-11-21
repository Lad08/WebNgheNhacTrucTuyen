using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebNgheNhacTrucTuyen.Data;
using WebNgheNhacTrucTuyen.Models;
using WebNgheNhacTrucTuyen.ViewModels;

namespace WebNgheNhacTrucTuyen.Controllers
{
    public class AlbumsController : Controller
    {
        private readonly AppDBContext _context;

        public AlbumsController(AppDBContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách album
        public async Task<IActionResult> Index()
        {
            var albums = await _context.Albums.Include(a => a.Songs).ToListAsync();
            return View(albums);
        }

        // Tạo album mới
        public IActionResult A_Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> A_Create(AlbumCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? coverImagePath = null;

                // Nếu có ảnh bìa được tải lên
                if (model.CoverImage != null)
                {
                    // Đường dẫn tới thư mục lưu ảnh bìa
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/album_img");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tạo tên file duy nhất và lưu file
                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.CoverImage.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImage.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn ảnh bìa (tương đối với wwwroot)
                    coverImagePath = $"/images/album_img/{uniqueFileName}";
                }

                // Tạo đối tượng Album và lưu vào cơ sở dữ liệu
                var album = new Album
                {
                    A_Name = model.Name,
                    A_Description = model.Description,
                    CoverImagePath = coverImagePath,
                    CreatedDate = DateTime.Now
                };

                _context.Add(album);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // Thêm bài hát vào album
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSong(int albumId, int songId)
        {
            var album = await _context.Albums
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(a => a.A_Id == albumId);
            var song = await _context.Songs.FindAsync(songId);

            if (album == null || song == null) return NotFound();

            // Kiểm tra bài hát đã tồn tại trong album chưa
            if (!album.Songs.Contains(song))
            {
                album.Songs.Add(song);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AddSong(int albumId)
        {
            var songs = await _context.Songs
                .Include(s => s.Artist)
                .ToListAsync();
            var viewModel = new AddSongViewModel
            {
                AlbumId = albumId,
                Songs = songs
            };
            return View(viewModel);
        }

    }
}
