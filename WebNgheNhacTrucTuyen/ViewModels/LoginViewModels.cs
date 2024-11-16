using System.ComponentModel.DataAnnotations;

namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class LoginViewModels
    {
        [Required(ErrorMessage = " Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = " Email is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
