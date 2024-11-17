using Microsoft.AspNetCore.Identity;

namespace WebNgheNhacTrucTuyen.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
    }
}
