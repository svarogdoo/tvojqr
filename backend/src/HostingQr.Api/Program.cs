using HostingQr.Api.Endpoints;
using HostingQr.Application;
using HostingQr.Infrastructure;
using HostingQr.Infrastructure.Migrations;

var builder = WebApplication.CreateBuilder(args);

string? port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "HostingQr API",
        Version = "v1",
        Description = "Backend foundation for HostingQr."
    });
});

var app = builder.Build();

await app.Services.RunDatabaseMigrationsAsync();

app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseCors("FrontendClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapSystemEndpoints();
app.MapProjectEndpoints();
app.MapSlugEndpoints();
app.MapPublicEndpoints();

app.Run();

public partial class Program;
