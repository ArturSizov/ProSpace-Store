using Microsoft.AspNetCore.Identity;

namespace ProSpace.Infrastructure.Entites.Users
{
    /// <summary>
    /// App user role
    /// </summary>
    public class AppUserRole : IdentityUserRole<Guid>
    {
        public AppUser UserApp { get; set; } = null!;
        public AppRole RoleApp { get; set; } = null!;
    }
}
