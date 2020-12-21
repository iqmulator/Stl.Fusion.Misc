using Stl;

namespace TodoApp.Services
{
    public class DbSession : IHasId<string>
    {
        public string Id { get; set; } = "";
        public string? UserId { get; set; }
        public bool IsSignOutForced { get; set; } = false;
    }
}
