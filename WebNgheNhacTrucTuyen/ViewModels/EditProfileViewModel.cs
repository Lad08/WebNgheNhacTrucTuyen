using System.ComponentModel.DataAnnotations;

namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class EditProfileViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; } // Email không chỉnh sửa
        public string PhoneNumber { get; set; } // Số điện thoại
        public string Address { get; set; } // Địa chỉ
        public DateTime? DateOfBirth { get; set; } // Ngày sinh
    }
}
