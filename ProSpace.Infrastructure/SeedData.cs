using Microsoft.AspNetCore.Identity;
using ProSpace.Infrastructure.Entites.Supply;
using ProSpace.Infrastructure.Entites.Users;
using ProSpace.Domain.Interfaces.Repositories;
using ProSpace.Domain.Models;

namespace ProSpace.Infrastructure
{
    /// <summary>
    ///  Creates sample data
    /// </summary>
    public static class SeedData
    {
        private static Random _random = new();

        public static async Task SeedUsersAsync(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IUnitOfWork unitOfWork)
        {
            if (roleManager.Roles.Any())
                return;

            var roles = new AppRole[]
            {
                new() { Name = "Manager" },
                new() { Name = "Customer" }
            };

            foreach (var role in roles)
                await roleManager.CreateAsync(role);

            var manager = new AppUser
            {
                Email = "manager@example.com",
                UserName = "manager@example.com",
                EmailConfirmed = true,
                Customer = new()
                {
                    Code = "0000-2025",
                    Name = "Manager"
                }
            };

            var johnDoe = new AppUser
            {
                Email = "johndoe@example.com",
                UserName = "johndoe@example.com",
                EmailConfirmed = true,
                Customer = new CustomerEntity
                {
                    Code = "0001-2025",
                    Name = "John Doe"
                }
            };

            var janeDoe = new AppUser
            {
                Email = "janedoe@example.com",
                UserName = "JaneDoe2",
                EmailConfirmed = true,
                Customer = new CustomerEntity
                {
                    Code = "0002-2025",
                    Name = "Jane Doe2"
                }
            };

            await userManager.CreateAsync(manager, "!P@$$w0rd1");
            await userManager.AddToRolesAsync(manager, ["Manager"]);

            await userManager.CreateAsync(johnDoe, "?KA{vrejW3^0=1Trrq");
            await userManager.AddToRolesAsync(johnDoe, ["Customer"]);

            await userManager.CreateAsync(janeDoe, "?KA{vrejW3^0=1Trrq");
            await userManager.AddToRolesAsync(janeDoe, ["Customer"]);

            await unitOfWork.CompleteAsync();
        }

        public static async Task SeedItemsAsync(IUnitOfWork unitOfWork)
        {
            var items = await unitOfWork.ItemsRepository.ReadAllAsync();

            if (items == null || items.Length > 20)
                return;

            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            for (var i = 0; i < 2; i++)
            {
                var symbol = alphabet[i];

                var item = new ItemModel 
                { 
                    Code= $"{i}{i}-{i}{i}{i}{i}-{symbol}{symbol}{i}{i}",
                    Name = $"Item {i}",
                    Price = i + 1, 
                    Category = $"Category {i++}"
                };

              await unitOfWork.ItemsRepository.CreateAsync(item);
            }
             await unitOfWork.CompleteAsync();
        }
    }
}
