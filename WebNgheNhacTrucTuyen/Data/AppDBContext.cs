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

        public DbSet<Album> Albums { get; set; }

        public DbSet<Artists> Artists { get; set; }
        public DbSet<Songs> Songs { get; set; }

        public DbSet<Genres> Genres { get; set; }

        public DbSet<Lyrics> Lyrics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ giữa Song và Genre
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Songs>()
                .HasOne(s => s.Genre)
                .WithMany(g => g.Songs)
                .HasForeignKey(s => s.GenreId);

            // Thiết lập quan hệ 1-N giữa Album và Songs
            modelBuilder.Entity<Songs>()
                .HasOne(s => s.Album)
                .WithMany(a => a.Songs)
                .HasForeignKey(s => s.AlbumId)
                .OnDelete(DeleteBehavior.SetNull); // Xóa album không xóa bài hát
        }
    }
}
