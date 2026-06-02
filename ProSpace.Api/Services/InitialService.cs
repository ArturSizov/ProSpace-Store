using Microsoft.AspNetCore.Identity;
using ProSpace.Application;
using ProSpace.Domain.Interfaces.Repositories;
using ProSpace.Application.Validations;
using ProSpace.Infrastructure;
using ProSpace.Infrastructure.Entites.Users;

namespace ProSpace.Api.Services
{
    public class InitialService : BackgroundService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ProSpaceDbContext _dataContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public InitialService(IServiceScopeFactory scopeFactory)
        {
            var scope = scopeFactory.CreateScope();

            _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            _dataContext = scope.ServiceProvider.GetRequiredService<ProSpaceDbContext>();
            _userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            _roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = await _dataContext.Database.EnsureCreatedAsync(stoppingToken);
            await SeedData.SeedUsersAsync(_userManager, _roleManager, _unitOfWork);
            await SeedData.SeedItemsAsync(_unitOfWork);
            //await SeedData.SeedCustomersAsync(_unitOfWork);
            //await SeedData.SeedOrdersAsync(_unitOfWork);
            //await SeedData.SeedOrderItemsAsync(_unitOfWork);
        }
    }
}
