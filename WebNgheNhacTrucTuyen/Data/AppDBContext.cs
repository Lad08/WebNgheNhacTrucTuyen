using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebNgheNhacTrucTuyen.Models;
using WebNgheNhacTrucTuyen.Models;

namespace WebNgheNhacTrucTuyen.Data
{
    public class AppDBContext : IdentityDbContext<Users>
    {
        public AppDBContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Users> Users { get; set; }
        public DbSet<Songs> Songs { get; set; }
    }
}
