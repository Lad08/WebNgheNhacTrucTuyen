namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public IList<string> Roles { get; set; }

        public bool IsBlocked { get; set; } // Thêm trạng thái
    }
}
