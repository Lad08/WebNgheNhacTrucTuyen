using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var albums = await _context.Albums
                .Include(a => a.Artist) // Bao gồm nghệ sĩ
                .Include(a => a.User) // Bao gồm người dùng
                .ToListAsync();

            ViewBag.UserId = userId; // Truyền thông tin UserId sang View

            return View(albums);
        }

        // Tạo album mới
        [HttpGet]
        public async Task<IActionResult> A_Create()
        {
            var artists = await _context.Artists.ToListAsync();

            var viewModel = new AlbumCreateViewModel
            {
                Artists = artists.Select(a => new SelectListItem
                {
                    Value = a.ART_Id.ToString(),
                    Text = a.ART_Name
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> A_Create(AlbumCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? coverImagePath = null;

                // Xử lý ảnh bìa nếu có
                if (model.CoverImage != null)
                {
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

                    coverImagePath = $"/images/album_img/{uniqueFileName}";
                }

                // Xác định nghệ sĩ hoặc người dùng là chủ sở hữu
                string? userId = null;
                int? artistId = null;

                if (model.ArtistId.HasValue && model.ArtistId.Value > 0)
                {
                    artistId = model.ArtistId; // Gắn nghệ sĩ nếu được chọn
                }
                else
                {
                    // Lấy ID của người dùng đang đăng nhập
                    userId = User.Identity.IsAuthenticated ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;

                    if (string.IsNullOrEmpty(userId))
                    {
                        ModelState.AddModelError("", "Bạn cần đăng nhập để tạo album hoặc chọn một nghệ sĩ.");
                        // Truyền lại danh sách nghệ sĩ nếu có lỗi
                        var artists = await _context.Artists.ToListAsync();
                        model.Artists = artists.Select(a => new SelectListItem
                        {
                            Value = a.ART_Id.ToString(),
                            Text = a.ART_Name
                        }).ToList();

                        return View(model);
                    }
                }

                // Tạo đối tượng Album
                var album = new Album
                {
                    A_Name = model.Name,
                    A_Description = model.Description,
                    A_CoverImagePath = coverImagePath,
                    A_CreatedDate = DateTime.Now,
                    ArtistId = artistId,
                    UserId = userId
                };

                _context.Add(album);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Truyền lại danh sách nghệ sĩ nếu có lỗi
            var artistsList = await _context.Artists.ToListAsync();
            model.Artists = artistsList.Select(a => new SelectListItem
            {
                Value = a.ART_Id.ToString(),
                Text = a.ART_Name
            }).ToList();

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

        // Hiển thị thông tin chi tiết của album
        public async Task<IActionResult> Details(int id)
        {
            var album = await _context.Albums
                .Include(a => a.Songs)
                .ThenInclude(s => s.Artist) // Bao gồm cả thông tin nghệ sĩ
                .FirstOrDefaultAsync(a => a.A_Id == id);

            if (album == null)
            {
                return NotFound();
            }

            // Chuyển Songs sang List
            album.Songs = album.Songs.ToList();

            return View(album);
        }

        // Xóa bài hát khỏi album
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSong(int albumId, int songId)
        {
            var album = await _context.Albums.Include(a => a.Songs)
                                             .FirstOrDefaultAsync(a => a.A_Id == albumId);
            if (album == null)
            {
                return NotFound();
            }

            var song = album.Songs.FirstOrDefault(s => s.S_Id == songId);
            if (song != null)
            {
                album.Songs.Remove(song);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = albumId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            var album = await _context.Albums.FindAsync(id);

            if (album == null)
            {
                return Json(new { success = false, message = "Album không tồn tại." });
            }

            album.IsFavoriteAlbum = !album.IsFavoriteAlbum;
            _context.Albums.Update(album);
            await _context.SaveChangesAsync();

            return Json(new { success = true, isFavorite = album.IsFavoriteAlbum });
        }


        public async Task<IActionResult> FavoriteAlbums()
        {
            // Lấy danh sách album yêu thích từ cơ sở dữ liệu
            var favoriteAlbums = await _context.Albums
                .Where(a => a.IsFavoriteAlbum)
                .ToListAsync();

            return View(favoriteAlbums);
        }


    }
}
