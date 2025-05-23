using System.Text;
using ECommerce.API.Extensions;
using ECommerce.API.Middleware;
using ECommerce.BLL.Options;
using ECommerce.BLL.ServiceContracts;
using ECommerce.BLL.Services;
using ECommerce.DAL.Constants;
using ECommerce.DAL.Data;
using ECommerce.DAL.Data.Repositories;
using ECommerce.DAL.Data.RepositoryContracts;
using ECommerce.DAL.Entities;
using ECommerce.DAL.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
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
            Array.Empty<string>()
        }
    });
});

RegisterApplicationServices(builder.Services);

builder.AddNpgsqlDbContext<ApplicationDbContext>("ecommercedb");

ConfigureIdentity(builder.Services);

ConfigureAuthentication(builder);

builder.Services.AddOptions<StripeConfigOptions>()
    .BindConfiguration(StripeConfigOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();


var app = builder.Build();

app.UseExceptionHandling();

app.MigrateDatabase<ApplicationDbContext>(async (context, services) =>
{
    await RoleSeeder.SeedRolesAsync(services);
});

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHsts();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#pragma warning disable S6966 // Awaitable method should be used
app.Run();
#pragma warning restore S6966 // Awaitable method should be used

void RegisterApplicationServices(IServiceCollection services)
{
    services.AddScoped<IProductRepository, ProductRepository>();
    services.AddScoped<ICartRepository, CartRepository>();
    services.AddScoped<IOrderRepository, OrderRepository>();

    services.AddScoped<IProductService, ProductService>();
    services.AddScoped<ICartService, CartService>();
    services.AddScoped<IOrderService, OrderService>();
    services.AddScoped<IPaymentService, PaymentService>();

    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IJwtService, JwtService>();
}

void ConfigureIdentity(IServiceCollection services)
{
    services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = EntityConstants.User.PasswordMinLength;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    builder.Services.AddOptions<JwtOptions>()
        .BindConfiguration(JwtOptions.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
            };
        });

    builder.Services.AddAuthorization();
}