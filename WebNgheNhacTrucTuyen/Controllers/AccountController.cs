using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using WebNgheNhacTrucTuyen.Models;
using WebNgheNhacTrucTuyen.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebNgheNhacTrucTuyen.Controllers
{
    public class AccountController : Controller
    {
		private readonly SignInManager<Users> signInManager;
		private readonly UserManager<Users> usersManager;

		public AccountController(SignInManager<Users> signInManager, UserManager<Users> usersManager)
		{
			this.signInManager = signInManager;
			this.usersManager = usersManager;
		}

		public IActionResult Login()
        {
            return View();
        }

		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModels model)
		{
			if (ModelState.IsValid)
			{
				var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

				if(result.Succeeded)
				{
                    // Kiểm tra xem người dùng có phải là admin không
                    var user = await signInManager.UserManager.FindByEmailAsync(model.Email);
                    var isAdmin = await signInManager.UserManager.IsInRoleAsync(user, "Admin");

                    if (isAdmin)
                    {
                        return RedirectToAction("Index", "Admin"); // Chuyển hướng đến Admin/Index
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home"); // Chuyển hướng đến trang chính
                    }
                }
				else
				{
					ModelState.AddModelError("", "Email or password is incorrect.");
					return View(model);
				}
			}
			return View(model);
		}

		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				Users users = new Users
				{
					FullName = model.Name,
					Email = model.Email,
					UserName = model.Email,
				};

				var result = await usersManager.CreateAsync(users, model.Password);

				if (result.Succeeded)
				{
                    var hasAdmin = await usersManager.GetUsersInRoleAsync("Admin");
                    if (hasAdmin == null || hasAdmin.Count == 0)
                    {
                        // Nếu chưa có Admin, gán quyền Admin cho tài khoản đầu tiên
                        await usersManager.AddToRoleAsync(users, "Admin");
                    }
                    else
                    {
                        // Nếu đã có Admin, gán quyền User mặc định
                        await usersManager.AddToRoleAsync(users, "User");
                    }

                    await signInManager.SignInAsync(users, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
				

		public IActionResult VerifyEmail()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
		{
			if(ModelState.IsValid)
			{
				var user = await usersManager.FindByNameAsync(model.Email);

				if (user == null) 
				{
					ModelState.AddModelError("", "Something is wrong!");
					return View(model);
				}
				else
				{
					return RedirectToAction("ChangePassword", "Account", new {username = user.UserName });
				}
			}
			return View(model);
		}

		public IActionResult ChangePassword(string username)
		{
			if (string.IsNullOrEmpty(username) )
			{
				return RedirectToAction("VerifyEmail", "Account");
			}
			return View(new ChangePasswordViewModel { Email = username });
		}

		[HttpPost]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await usersManager.FindByNameAsync(model.Email);
				if(user != null)
				{
					var result = await usersManager.RemovePasswordAsync(user);
					if (result.Succeeded)
					{
						result = await usersManager.AddPasswordAsync(user, model.NewPassword);
						return RedirectToAction("Login", "Account");
					}
					else
					{
						foreach (var error in result.Errors)
						{
							ModelState.AddModelError("", error.Description);
						}

						return View(model);
					}
				}
				else
				{
					ModelState.AddModelError("", "Email not found!");
					return View(model);
				}
			}
			else
			{
				ModelState.AddModelError("", "Something went wrong. Try again.");
				return View(model);
			}
		}

		public async Task<IActionResult> Logout()
		{
			await signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

        public async Task<IActionResult> Index()
        {
            var users = usersManager.Users.ToList(); // Lấy danh sách người dùng
            return View(users);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<Users> GetCurrentUserAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await usersManager.FindByIdAsync(userId);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            var model = new EditProfileViewModel
            {
                FullName = currentUser.FullName,
                Email = currentUser.Email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return RedirectToAction("Login");
                }

                currentUser.FullName = model.FullName;

                // Email không được phép chỉnh sửa nếu bạn muốn giữ nguyên logic của Identity Framework
                var result = await usersManager.UpdateAsync(currentUser);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

    }
}
