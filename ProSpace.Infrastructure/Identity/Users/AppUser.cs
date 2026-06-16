using Microsoft.AspNetCore.Identity;
using ProSpace.Infrastructure.Entites.Supply;

namespace ProSpace.Infrastructure.Entites.Users
{
    public class AppUser : IdentityUser<Guid>
    {
        public CustomerEntity? Customer { get; set; }

        public ICollection<AppUserRole> UserRoles { get; set; } = [];
    }
}
