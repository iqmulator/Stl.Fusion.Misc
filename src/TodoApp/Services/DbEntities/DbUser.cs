using Stl;

namespace TodoApp.Services
{
    public class DbUser : IHasId<string>
    {
        public string Id { get; set; } = "";
        public string AuthenticationType { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string ClaimsJson { get; set; } = "";
    }
}
