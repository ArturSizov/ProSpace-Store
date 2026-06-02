using Microsoft.AspNetCore.Identity;
using ProSpace.Infrastructure.Entites.Supply;

namespace ProSpace.Infrastructure.Entites.Users
{
    public class AppUser : IdentityUser<Guid>
    {
        public Guid CustomerId { get; set; }

        public CustomerEntity Customer { get; set; } = null!;

        public ICollection<AppUserRole> UserRoles { get; set; } = [];
    }
}
