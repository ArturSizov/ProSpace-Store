using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProSpace.Api.Services;
using ProSpace.Application.Common.Interfaces;
using ProSpace.Application.Interfaces.Repositories;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Application.Services;
using ProSpace.Application.Validations;
using ProSpace.Infrastructure;
using ProSpace.Infrastructure.Entites.Users;
using ProSpace.Infrastructure.Identity.Services;
using ProSpace.Infrastructure.Repositories;
using ProSpace.Infrastructure.Services;
using System.Text;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // For Entity Framework
        var configuration = builder.Configuration;

        // --- DATABASE & IDENTITY CONFIGURATION ---
        builder.Services.AddDbContext<ProSpaceDbContext>(options =>
        {
            options.UseSqlite(builder.Configuration.GetConnectionString(nameof(ProSpaceDbContext)));
        });


        // Replaced AddIdentityApiEndpoints with standard AddIdentity to prevent JWT authentication conflicts
        builder.Services.AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<ProSpaceDbContext>()
            .AddDefaultTokenProviders();

        // AUTHENTICATION & AUTHORIZATION
        builder.Services.AddAuthorization();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
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

        builder.Services.AddIdentityCore<AppUser>()
                .AddRoles<AppRole>()
                .AddEntityFrameworkStores<ProSpaceDbContext>()
                .AddDefaultTokenProviders();

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

        builder.Services.AddHttpContextAccessor();

        builder.Services
            .AddScoped<IItemsRepository, ItemsRepository>()
            .AddScoped<IOrderItemsRepository, OrderItemsRepository>()
            .AddScoped<IOrdersRepository, OrdersRepository>()
            .AddScoped<ICustomersRepository, CustomerRepository>()
            .AddScoped<IUnitOfWork, UnitOfWork>();

        // SERVICES (BLL) 
        builder.Services
            .AddScoped<IIdentityService, IdentityService>()
            .AddScoped<IItemsService, ItemsService>()
            .AddScoped<IOrderItemsService, OrderItemsService>()
            .AddScoped<IOrderService, OrdersService>()
            .AddScoped<ICustomersService, CustomersService>()
            .AddScoped<ITokenService, TokenService>();

        // VALIDATORS
        builder.Services.AddValidatorsFromAssemblyContaining<RegisterCustomerValidator>();

        //BACKGROUND SERVICES
        builder.Services.AddHostedService<InitialService>();

        // CONTROLLERS & SWAGGER
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Auto API", Version = "V1" });

            options.SchemaFilter<ProSpace.Api.Infrastructure.Swagger.BaseResponseSchemaFilter>();

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

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });


        var app = builder.Build();

        // Enable CORS
        app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}