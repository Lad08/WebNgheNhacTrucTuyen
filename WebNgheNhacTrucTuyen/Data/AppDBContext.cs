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

        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }

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

            // Cấu hình quan hệ nhiều-nhiều giữa Playlist và Songs thông qua PlaylistSong
            modelBuilder.Entity<PlaylistSong>()
                .HasKey(ps => ps.Id); // Khóa chính của bảng trung gian

            modelBuilder.Entity<PlaylistSong>()
                .HasOne(ps => ps.Playlist)
                .WithMany(p => p.PlaylistSongs) // Một Playlist có nhiều PlaylistSong
                .HasForeignKey(ps => ps.PlaylistId)
                .OnDelete(DeleteBehavior.NoAction); // Khi Playlist bị xóa, các PlaylistSong liên quan cũng bị xóa

            modelBuilder.Entity<PlaylistSong>()
                .HasOne(ps => ps.Song)
                .WithMany(s => s.PlaylistSongs) // Một Song có thể nằm trong nhiều PlaylistSong
                .HasForeignKey(ps => ps.SongId)
                .OnDelete(DeleteBehavior.NoAction); // Khi Song bị xóa, các PlaylistSong liên quan cũng bị xóa

            // Thiết lập quan hệ giữa Album và Artists
            modelBuilder.Entity<Album>()
                .HasOne(a => a.Artist)
                .WithMany(a => a.Albums) // Thêm property Albums vào Artists nếu cần
                .HasForeignKey(a => a.ArtistId)
                .OnDelete(DeleteBehavior.SetNull); // Xóa nghệ sĩ không xóa album


        }
    }
}
