using HostingQr.Api.Endpoints;
using HostingQr.Application;
using HostingQr.Infrastructure;
using HostingQr.Infrastructure.Configuration;
using HostingQr.Infrastructure.Migrations;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

string? port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
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

app.UseForwardedHeaders();
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
var storageOptions = app.Services.GetRequiredService<IOptions<StorageOptions>>().Value;
if (!storageOptions.UsesR2() && !string.IsNullOrWhiteSpace(storageOptions.UploadsRootPath))
{
    Directory.CreateDirectory(storageOptions.UploadsRootPath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(storageOptions.UploadsRootPath),
        RequestPath = "/uploads"
    });
}
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
