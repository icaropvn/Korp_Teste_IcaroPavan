using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("gateway-cors", p => p
        .WithOrigins(allowedOrigins.Length > 0 ? allowedOrigins : new[] { "http://localhost:4200" })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.WebHost.UseUrls("http://localhost:8080");

var app = builder.Build();

app.UseCors("gateway-cors");
app.UseForwardedHeaders();

app.MapGet("/health", () => Results.Ok(new { ok = true, service = "gateway" }));

app.MapReverseProxy();

app.Run();
