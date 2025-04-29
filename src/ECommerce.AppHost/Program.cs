var builder = DistributedApplication.CreateBuilder(args);

var postgresServer = builder.AddPostgres("postgresdb")
    .WithDataVolume();
var database = postgresServer.AddDatabase("ecommercedb");
builder.AddProject<Projects.ECommerce_API>("ecommerce-api")
    .WithEnvironment("Jwt__Secret", builder.Configuration["Jwt:Secret"])
    .WithEnvironment("Jwt__Issuer", builder.Configuration["Jwt:Issuer"])
    .WithEnvironment("Jwt__Audience", builder.Configuration["Jwt:Audience"])
    .WithEnvironment("Jwt__ExpirationMinutes", builder.Configuration["Jwt:ExpirationMinutes"])
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
