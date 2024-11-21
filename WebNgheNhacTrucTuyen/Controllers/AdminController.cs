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
                    Roles = rolesForUser
                });
            }

            return View(userRoles);
        }





        // Hiển thị trang chỉnh sửa người dùng
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var rolesForUser = await _userManager.GetRolesAsync(user);
            var model = new UserManageViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = rolesForUser.ToList(),
                AllRoles = _roleManager.Roles.Select(r => r.Name).ToList()
            };

            return View(model);
        }

        // Xử lý yêu cầu chỉnh sửa người dùng
        [HttpPost]
        public async Task<IActionResult> EditUser(UserManageViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = model.UserName;
            user.Email = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Error updating user.");
                return View(model);
            }

            // Cập nhật vai trò
            var userRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = model.Roles.Except(userRoles).ToList();
            var rolesToRemove = userRoles.Except(model.Roles).ToList();

            await _userManager.AddToRolesAsync(user, rolesToAdd);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            return RedirectToAction(nameof(Index));
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
                .FirstOrDefaultAsync(s => s.Id == id);

            if (song == null)
            {
                return NotFound();
            }

            var model = new EditSongViewModel
            {
                Id = song.Id,
                Title = song.Title,
                UploadDate = song.UploadDate,
                IsFavorite = song.IsFavorite,
                ArtistName = song.Artist.Name, // Nhập tên nghệ sĩ
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

            var song = await _context.Songs.Include(s => s.Artist).FirstOrDefaultAsync(s => s.Id == model.Id);

            if (song == null)
            {
                return NotFound();
            }

            song.Title = model.Title;
            song.UploadDate = model.UploadDate;
            song.IsFavorite = model.IsFavorite;

            // Tìm hoặc tạo nghệ sĩ mới
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.Name == model.ArtistName);
            if (artist == null)
            {
                artist = new Artists { Name = model.ArtistName };
                _context.Artists.Add(artist);
                await _context.SaveChangesAsync();
            }
            song.ArtistId = artist.Id;


            song.GenreId = model.GenreId;
            song.AlbumId = model.AlbumId;

            _context.Songs.Update(song);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Admin");
        }

    }
}
