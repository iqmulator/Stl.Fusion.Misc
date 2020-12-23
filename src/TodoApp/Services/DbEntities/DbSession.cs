using System;
using Stl;

namespace TodoApp.Services
{
    public class DbSession : IHasId<string>
    {
        public string Id { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime LastSeenAt { get; set; }
        public string IPAddress { get; set; } = "";
        public string UserAgent { get; set; } = "";
        public string ExtraPropertiesJson { get; set; } = "";
        public string? UserId { get; set; }
        public bool IsSignOutForced { get; set; } = false;
    }
}
