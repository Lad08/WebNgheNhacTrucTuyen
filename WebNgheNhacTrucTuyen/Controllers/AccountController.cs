using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
					return RedirectToAction("Index", "Home");
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
					
					Email = model.Email,
					UserName = model.Email,
				};

				var result = await usersManager.CreateAsync(users, model.Password);

				if (result.Succeeded)
				{
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


    }
}
