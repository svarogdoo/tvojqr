using HostingQr.Api.Endpoints;
using HostingQr.Application;
using HostingQr.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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

app.UseSwagger();
app.UseSwaggerUI();

app.MapSystemEndpoints();

app.Run();

public partial class Program;
