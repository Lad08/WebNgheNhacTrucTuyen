using WebNgheNhacTrucTuyen.Models;

namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; }
        public List<Songs> Songs { get; set; }
        public List<Artists> Artists { get; set; }
    }
}
