using ECommerce.API.Extensions;
using ECommerce.DAL.Data;
using ECommerce.DAL.Data.Repositories;
using ECommerce.DAL.Data.RepositoryContracts;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.AddNpgsqlDbContext<ApplicationDbContext>("ecommercedb");

var app = builder.Build();

app.MigrateDatabase<ApplicationDbContext>();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
