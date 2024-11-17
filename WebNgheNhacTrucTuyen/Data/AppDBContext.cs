using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebNgheNhacTrucTuyen.Models;


namespace WebNgheNhacTrucTuyen.Data
{
    public class AppDBContext : IdentityDbContext<Users>
    {
        public AppDBContext(DbContextOptions options) : base(options)
        {

        }

        
        public DbSet<Songs> Songs { get; set; }

        public DbSet<Genres> Genres { get; set; }

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ giữa Song và Genre
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Songs>()
                .HasOne(s => s.Genre)
                .WithMany(g => g.Songs)
                .HasForeignKey(s => s.GenreId);


        }
    }
}
