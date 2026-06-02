using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProSpace.Api.Services;
using ProSpace.Infrastructure;
using ProSpace.Infrastructure.Repositories;
using ProSpace.Domain.Interfaces.Repositories;
using ProSpace.Domain.Interfaces.Services;
using ProSpace.Domain.Interfaces.Validations;
using ProSpace.Domain.Models;
using ProSpace.Application.Services;
using ProSpace.Application.Validations;
using ProSpace.Application.Validations.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProSpace.Infrastructure.Entites.Users;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // For Entity Framework
        var configuration = builder.Configuration;

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddDbContext<ProSpaceDbContext>(
                        options => {  options.UseSqlite(builder.Configuration.GetConnectionString(nameof(ProSpaceDbContext))); })
                        .AddIdentityApiEndpoints<AppUser>()
                        .AddRoles<AppRole>()
                        .AddEntityFrameworkStores<ProSpaceDbContext>()
                        .AddDefaultTokenProviders();

        builder.Services.AddAuthorization();

        // Adding Authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = configuration["JWT:ValidAudience"],
                ValidIssuer = configuration["JWT:ValidIssuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"] ?? 
                                                                                   throw new Exception("JWT secret not found")))
            };
        });


        builder.Services.Configure<IdentityOptions>(options =>
        {
            // Password settings
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = false;

            //SignIn settings
            options.SignIn.RequireConfirmedEmail = false;

            // User settings
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;

        });

        // repositories
        builder.Services
            .AddScoped<IItemsRepository, ItemsRepository>()
            .AddScoped<IOrderItemsRepository, OrderItemsRepository>()
            .AddScoped<IOrdersRepository, OrdersRepository>()
            .AddScoped<ICustomersRepository, CustomerRepository>()
            .AddScoped<IUnitOfWork, UnitOfWork>();

        // domain services
        builder.Services
            .AddScoped<IItemsService, ItemsService>()
            .AddScoped<IOtderItemsService, OrderItemsService>()
            .AddScoped<IOrderService, OrdersService>()
            .AddScoped<ICustomersService, CustomersService>()
            .AddScoped<RoleManager<AppRole>>();

        // validation services
        builder.Services
            .AddScoped<IValidationProvider<CustomerModel>, CustomerValidationsService>()
            .AddScoped<IValidator<CustomerModel>, CustomerValidations>()

            .AddScoped<IValidationProvider<ItemModel>, ItemValidationsService>()
            .AddScoped<IValidator<ItemModel>, ItemValidations>()

            .AddScoped<IValidationProvider<OrderModel>, OrderValidationsService>()
            .AddScoped<IValidator<OrderModel>, OrderValidations>()

            .AddScoped<IValidationProvider<OrderItemModel>, OrderItemValidationsService>()
            .AddScoped<IValidator<OrderItemModel>, OrderItemValidations>();

        // background services
        builder.Services.AddHostedService<InitialService>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Auto API", Version = "V1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        var app = builder.Build();

        //Enable CORS
        app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        //app.MapIdentityApi<AppUser>();

        await app.RunAsync();
    }
}