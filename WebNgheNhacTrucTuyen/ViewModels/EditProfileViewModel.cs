using System.ComponentModel.DataAnnotations;

namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class EditProfileViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
