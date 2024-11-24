namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class EditLyricsViewModel
    {
        public int Id { get; set; }
        public int SongId { get; set; }
        public string? CurrentLyricsContent { get; set; }
        public string? LyricsFilePath { get; set; }
        public string? NewLyricsContent { get; set; }
        public IFormFile? NewLyricsFile { get; set; }
    }
}
