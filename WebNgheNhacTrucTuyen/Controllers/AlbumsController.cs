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
                    A_CoverImagePath = coverImagePath,
                    A_CreatedDate = DateTime.Now
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
        // Hiển thị xác nhận xóa
        public async Task<IActionResult> Delete(int id)
        {
            var album = await _context.Albums
                .Include(a => a.Songs) // Nếu muốn hiển thị số bài hát trong album
                .FirstOrDefaultAsync(a => a.A_Id == id);

            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // Xử lý xóa album
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var album = await _context.Albums
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(a => a.A_Id == id);

            if (album == null)
            {
                return NotFound();
            }

            // Xóa bài hát khỏi album (không xóa bài hát khỏi hệ thống)
            foreach (var song in album.Songs)
            {
                song.AlbumId = null;
            }
            if (!string.IsNullOrEmpty(album.A_CoverImagePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", album.A_CoverImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _context.Albums.Remove(album);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Hiển thị form chỉnh sửa album
        public async Task<IActionResult> Edit(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }

            // Tạo ViewModel để hiển thị thông tin trong form
            var viewModel = new AlbumEditViewModel
            {
                Id = album.A_Id,
                Name = album.A_Name,
                Description = album.A_Description,
                ExistingCoverImagePath = album.A_CoverImagePath
            };

            return View(viewModel);
        }

        // Xử lý cập nhật album
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AlbumEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var album = await _context.Albums.FindAsync(id);
                if (album == null)
                {
                    return NotFound();
                }

                album.A_Name = model.Name;
                album.A_Description = model.Description;

                // Xử lý cập nhật ảnh bìa
                if (model.CoverImage != null)
                {
                    // Xóa ảnh bìa cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(album.A_CoverImagePath))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", album.A_CoverImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Lưu ảnh bìa mới
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/album_img");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.CoverImage.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImage.CopyToAsync(stream);
                    }

                    album.A_CoverImagePath = $"/images/album_img/{uniqueFileName}";
                }

                _context.Update(album);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}
