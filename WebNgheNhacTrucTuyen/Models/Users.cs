using Microsoft.AspNetCore.Identity;

namespace WebNgheNhacTrucTuyen.Models
{
    public class Users : IdentityUser
    {
        public string U_FullName { get; set; }

        public string? U_Address { get; set; } // Địa chỉ
        public DateTime? U_DateOfBirth { get; set; } // Ngày sinh
        public bool IsBlocked { get; set; } = false;
    }
}
