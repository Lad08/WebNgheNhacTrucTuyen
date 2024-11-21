namespace WebNgheNhacTrucTuyen.ViewModels
{
    public class UserManageViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public List<string> AllRoles { get; set; }
    }
}
