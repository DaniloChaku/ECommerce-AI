var builder = DistributedApplication.CreateBuilder(args);

var postgresServer = builder.AddPostgres("postgresdb")
    .WithDataVolume();
var database = postgresServer.AddDatabase("ecommercedb");
builder.AddProject<Projects.ECommerce_API>("ecommerce-api")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
