namespace NuJournalPro.Models.Identity
{
    public class UserInfo
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public List<string> UserRoles { get; set; } = new List<string>();
        public string UserRolesString
        {
            get
            {
                return string.Join(", ", UserRoles);
            }
        }
    }
}
