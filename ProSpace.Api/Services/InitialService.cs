using Microsoft.AspNetCore.Identity;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Infrastructure;
using ProSpace.Infrastructure.Entites.Users;

namespace ProSpace.Api.Services
{
    /// <summary>
    /// Long-running background service responsible for orchestrating initial system startup database migrations,
    /// verification states, and default corporate identity profile seedling operations.
    /// </summary>
    public class InitialService : BackgroundService
    {
        /// <summary>
        /// The factory infrastructure component utilized to dynamically generate isolated execution scope blocks 
        /// to safely resolve short-lived Scoped database dependencies inside long-running Singleton workflows.
        /// </summary>
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitialService"/> class.
        /// </summary>
        /// <param name="scopeFactory">The root service provider application factory enabling dynamic dependency boundary creation.</param>
        public InitialService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Executes the background data preparation lifecycle thread.
        /// Ensures relational physical tables exist and populates the schema with baseline system entities.
        /// </summary>
        /// <param name="stoppingToken">The token structure monitoring for application cluster shutdown signals.</param>
        /// <returns>A tracking Task tracking completing state alignment loops.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Create a temporary isolated container to safely resolve and dispose of Scoped database resources
            using var scope = _scopeFactory.CreateScope();

            // Extract context layers safely inside the active scoped transaction lifespan loop block boundary
            var dataContext = scope.ServiceProvider.GetRequiredService<ProSpaceDbContext>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

            // Ensure physical data storage engines are fully provisioned and structures are compiled
            _ = await dataContext.Database.EnsureCreatedAsync(stoppingToken);

            // Execute operational database tables seeding workflows sequentially
            await SeedData.SeedUsersAsync(userManager, roleManager, unitOfWork);
            await SeedData.SeedItemsAsync(unitOfWork);

            // Contextual future workspace indicators reserved for workflow staging updates:
            // await SeedData.SeedCustomersAsync(unitOfWork);
            // await SeedData.SeedOrdersAsync(unitOfWork);
            // await SeedData.SeedOrderItemsAsync(unitOfWork);
        }
    }
}
