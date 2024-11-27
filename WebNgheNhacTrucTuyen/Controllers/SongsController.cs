using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebNgheNhacTrucTuyen.Models;
using WebNgheNhacTrucTuyen.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebNgheNhacTrucTuyen.ViewModels;


namespace WebNgheNhacTrucTuyen.Controllers
{
    [Authorize(Roles = "User,Admin ")]
    public class SongsController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<SongsController> _logger;
        private static List<Songs> _queue = new List<Songs>();
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
            ViewBag.Artists = _context.Artists.ToList(); // Lấy danh sách nghệ sĩ
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, IFormFile coverImage, string title, int artistId, int genreId)
        {
            if (file == null || file.Length == 0 || coverImage == null || coverImage.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file nhạc và ảnh bìa hợp lệ.");
                return View();
            }

            // Lấy thông tin người dùng đăng nhập
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Kiểm tra nghệ sĩ
            var artist = await _context.Artists.FindAsync(artistId);
            if (artist == null)
            {
                ModelState.AddModelError("", "Nghệ sĩ không tồn tại.");
                return View();
            }

            // Tạo thư mục lưu file nhạc nếu chưa tồn tại
            var filePath = Path.Combine(_environment.WebRootPath, "music", file.FileName);
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            // Lưu file nhạc
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Tạo thư mục lưu ảnh bìa nếu chưa tồn tại
            var coverImagePath = Path.Combine(_environment.WebRootPath, "images/song_img", coverImage.FileName);
            if (!Directory.Exists(Path.GetDirectoryName(coverImagePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(coverImagePath));
            }

            // Lưu ảnh bìa
            using (var stream = new FileStream(coverImagePath, FileMode.Create))
            {
                await coverImage.CopyToAsync(stream);
            }

            // Lưu thông tin bài hát vào cơ sở dữ liệu
            var song = new Songs
            {
                S_Title = title,
                S_FilePath = "/music/" + file.FileName,
                S_CoverImagePath = "/images/song_img/" + coverImage.FileName,
                UserId = user.Id,
                S_UploadDate = DateTime.Now,
                GenreId = genreId,
                ArtistId = artistId // Liên kết với nghệ sĩ
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Bài hát đã được tải lên thành công.";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> FavoriteSongs(string genre)
        {
            // Lấy thông tin người dùng hiện tại
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy tất cả thể loại
            var genres = await _context.Genres.ToListAsync();
            ViewBag.Genres = genres;

            // Lấy bài hát yêu thích
            var favoriteSongsQuery = _context.Songs
                .Include(s => s.Genre)
                .Include(s => s.Artist)
                .Include(s => s.Album)
                .Where(s => s.S_IsFavorite);

            // Nếu genre không null, lọc thêm theo thể loại
            if (!string.IsNullOrEmpty(genre))
            {
                favoriteSongsQuery = favoriteSongsQuery.Where(s => s.Genre.G_Name == genre);
            }

            // Lấy danh sách bài hát yêu thích
            var favoriteSongs = await favoriteSongsQuery.ToListAsync();

            // Truyền danh sách bài hát yêu thích vào ViewBag
            ViewBag.FavoriteSongs = favoriteSongs;

            // Trả về danh sách bài hát yêu thích để hiển thị
            return View(favoriteSongs);
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
            var filePath = Path.Combine(_environment.WebRootPath, song.S_FilePath.TrimStart('/'));
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
                   .Include(s => s.User)      // Bao gồm User
                   .Include(s => s.Genre)     // Bao gồm thể loại
                   .Include(s => s.Lyrics)    // Bao gồm lyrics
                   .Include(s => s.Album)     // Bao gồm Album
                   .Include(s => s.Artist)    // Bao gồm Nghệ sĩ
                   .FirstOrDefaultAsync(s => s.S_Id == id);

            if (song == null)
            {
                return NotFound("Bài hát không tồn tại.");
            }

            // Xử lý lyrics (từ file hoặc nội dung trực tiếp)
            if (song.Lyrics != null)
            {
                if (!string.IsNullOrEmpty(song.Lyrics.L_FilePath))
                {
                    string fullPath = Path.Combine(_environment.WebRootPath, song.Lyrics.L_FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        ViewBag.LyricsContent = System.IO.File.ReadAllText(fullPath);
                    }
                }
                else if (!string.IsNullOrEmpty(song.Lyrics.L_Content))
                {
                    ViewBag.LyricsContent = song.Lyrics.L_Content; // Lấy nội dung trực tiếp từ DB
                }
            }

            return View(song);
        }

        // Chức năng bài hát yêu thích
        public IActionResult ToggleFavorite(int id)
        {
            var song = _context.Songs.Find(id);
            if (song != null)
            {
                song.S_IsFavorite = !song.S_IsFavorite;
                _context.SaveChanges();
            }
            return RedirectToAction("FavoriteSongs");
        }


        public async Task<IActionResult> EditSong(int id)
        {
            var song = await _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Genre)
                .Include(s => s.Album)
                .FirstOrDefaultAsync(s => s.S_Id == id);

            if (song == null)
            {
                return NotFound();
            }

            var model = new EditSongViewModel
            {
                Id = song.S_Id,
                Title = song.S_Title,
                ExistingCoverImagePath = song.S_CoverImagePath,
                UploadDate = song.S_UploadDate,
                ArtistName = song.Artist.ART_Name, // Nhập tên nghệ sĩ
                GenreId = song.GenreId,
                AlbumId = song.AlbumId
            };

            var genres = await _context.Genres.ToListAsync();
            var albums = await _context.Albums.ToListAsync();

            ViewBag.Genres = new SelectList(genres, "G_Id", "G_Name");
            ViewBag.Albums = new SelectList(
                albums.Prepend(new Album { A_Id = 0, A_Name = "Không thuộc album nào" }),
                "A_Id",
                "A_Name",
                model.AlbumId
            );


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditSong(EditSongViewModel model, int id)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                var genres = await _context.Genres.ToListAsync();
                var albums = await _context.Albums.ToListAsync();
                ViewBag.Genres = new SelectList(genres, "G_Id", "G_Name");
                ViewBag.Albums = new SelectList(
                    albums.Prepend(new Album { A_Id = 0, A_Name = "Không thuộc album nào" }),
                    "A_Id",
                    "A_Name",
                    model.AlbumId
                );
                return View(model);
            }

            var song = await _context.Songs.Include(s => s.Artist).FirstOrDefaultAsync(s => s.S_Id == model.Id);

            if (song == null)
            {
                return NotFound();
            }

            song.S_Title = model.Title;
            song.S_UploadDate = model.UploadDate;

            // Xử lý cập nhật ảnh bìa
            if (model.CoverImage != null)
            {
                if (!string.IsNullOrEmpty(song.S_CoverImagePath))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", song.S_CoverImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/song_img");
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

                song.S_CoverImagePath = $"/images/song_img/{uniqueFileName}";
            }

            // Tìm hoặc tạo nghệ sĩ mới
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.ART_Name == model.ArtistName);
            if (artist == null)
            {
                artist = new Artists { ART_Name = model.ArtistName };
                _context.Artists.Add(artist);
                await _context.SaveChangesAsync();
            }
            song.ArtistId = artist.ART_Id;

            song.GenreId = model.GenreId;

            // Xử lý AlbumId (cho phép null)
            song.AlbumId = model.AlbumId == 0 ? null : model.AlbumId;

            _context.Songs.Update(song);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult UploadLyrics(int songId)
        {
            var model = new UploadLyricsViewModel
            {
                SongId = songId
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadLyrics(UploadLyricsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var song = await _context.Songs.Include(s => s.Lyrics).FirstOrDefaultAsync(s => s.S_Id == model.SongId);
            if (song == null)
            {
                return NotFound("Bài hát không tồn tại.");
            }

            var lyrics = new Lyrics { SongId = model.SongId };

            if (model.File != null)
            {
                var filePath = Path.Combine(_environment.WebRootPath, "lyrics", model.File.FileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                lyrics.L_FilePath = "/lyrics/" + model.File.FileName;
            }
            else if (!string.IsNullOrEmpty(model.Content))
            {
                var fileName = $"{Guid.NewGuid()}.txt";
                var filePath = Path.Combine(_environment.WebRootPath, "lyrics", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                await System.IO.File.WriteAllTextAsync(filePath, model.Content);

                lyrics.L_FilePath = "/lyrics/" + fileName;
            }

            _context.Lyrics.Add(lyrics);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Lyrics đã được tải lên thành công.";
            return RedirectToAction("Details", new { id = model.SongId });
        }

        [HttpGet]
        public async Task<IActionResult> EditLyrics(int id)
        {
            var lyrics = await _context.Lyrics.Include(l => l.Song).FirstOrDefaultAsync(l => l.L_Id == id);
            if (lyrics == null)
            {
                return NotFound("Lyrics không tồn tại.");
            }

            var model = new UploadLyricsViewModel
            {
                SongId = lyrics.SongId,
                ExistingFilePath = lyrics.L_FilePath
            };

            if (!string.IsNullOrEmpty(lyrics.L_FilePath))
            {
                var fullPath = Path.Combine(_environment.WebRootPath, lyrics.L_FilePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    model.Content = await System.IO.File.ReadAllTextAsync(fullPath);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLyrics(UploadLyricsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var lyrics = await _context.Lyrics.Include(l => l.Song).FirstOrDefaultAsync(l => l.SongId == model.SongId);
            if (lyrics == null)
            {
                return NotFound("Lyrics không tồn tại.");
            }

            if (model.File != null)
            {
                var filePath = Path.Combine(_environment.WebRootPath, "lyrics", model.File.FileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(lyrics.L_FilePath))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, lyrics.L_FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                lyrics.L_FilePath = "/lyrics/" + model.File.FileName;
            }
            else if (!string.IsNullOrEmpty(model.Content))
            {
                var filePath = Path.Combine(_environment.WebRootPath, lyrics.L_FilePath.TrimStart('/'));
                await System.IO.File.WriteAllTextAsync(filePath, model.Content);
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Lyrics đã được cập nhật.";
            return RedirectToAction("Details", new { id = model.SongId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLyrics(int id)
        {
            var lyrics = await _context.Lyrics.FirstOrDefaultAsync(l => l.L_Id == id);
            if (lyrics == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(lyrics.L_FilePath))
            {
                var filePath = Path.Combine(_environment.WebRootPath, lyrics.L_FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Lyrics.Remove(lyrics);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Lyrics đã được xóa.";
            return RedirectToAction("Details", new { id = lyrics.SongId });
        }





    }
}
