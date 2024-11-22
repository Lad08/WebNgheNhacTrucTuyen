using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using WebNgheNhacTrucTuyen.Data;
using WebNgheNhacTrucTuyen.Models;
using WebNgheNhacTrucTuyen.ViewModels;

namespace WebNgheNhacTrucTuyen.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDBContext _context;
        public AdminController(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager, AppDBContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var roles = _roleManager.Roles.ToList();

            var userRoles = new List<UserRoleViewModel>();
            foreach (var user in users)
            {
                var rolesForUser = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = rolesForUser,
                    IsBlocked = user.IsBlocked
                });
            }

            return View(userRoles);
        }


        // Xóa người dùng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSong(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null) return NotFound();

            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Songs()
        {
            var songs = await _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Genre)
                .Include(s => s.Album)
                .Include(s => s.User)
                .ToListAsync();

            return View(songs);
        }

        [HttpGet]
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
                UploadDate = song.S_UploadDate,
                ArtistName = song.Artist.ART_Name, // Nhập tên nghệ sĩ
                GenreId = song.GenreId,
                AlbumId = song.AlbumId
            };

            var genres = await _context.Genres.ToListAsync();
            var albums = await _context.Albums.ToListAsync();

            ViewBag.Genres = genres.Any()
                ? new SelectList(genres, "G_Id", "G_Name")
                : new SelectList(new List<SelectListItem> { new SelectListItem { Text = "No Genres Available", Value = "0" } });

            ViewBag.Albums = albums.Any()
                ? new SelectList(albums, "A_Id", "A_Name")
                : new SelectList(new List<SelectListItem> { new SelectListItem { Text = "No Albums Available", Value = "0" } });

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditSong(EditSongViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Genres = new SelectList(_context.Genres, "G_Id", "G_Name");
                ViewBag.Albums = new SelectList(_context.Albums, "A_Id", "A_Name");
                return View(model);
            }

            var song = await _context.Songs.Include(s => s.Artist).FirstOrDefaultAsync(s => s.S_Id == model.Id);

            if (song == null)
            {
                return NotFound();
            }

            song.S_Title = model.Title;
            song.S_UploadDate = model.UploadDate;
            song.S_IsFavorite = model.IsFavorite;

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
            song.AlbumId = model.AlbumId;

            _context.Songs.Update(song);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsBlocked = !user.IsBlocked; // Đổi trạng thái Block/Unblock
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
