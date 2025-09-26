using SMARTMOB_PANTAREI_BACK.Data;
using SMARTMOB_PANTAREI_BACK.Hubs;
using SMARTMOB_PANTAREI_BACK.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IAcquisizioniService, AcquisizioniService>();
builder.Services.AddScoped<IPostazioniService, PostazioniService>();
builder.Services.AddScoped<IPostazioniPerLineaService, PostazioniPerLineaService>();
builder.Services.AddScoped<IAcquisizioniRealtimeService, AcquisizioniRealtimeService>();


// Add SignalR before registering hosted service
builder.Services.AddSignalR();
builder.Services.AddHostedService<AcquisizioniRealtimeService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    // More restrictive policy for production (optional)
    options.AddPolicy("AllowSpecific", policy =>
    {
        policy.SetIsOriginAllowed(origin => 
            {
                // Allow any localhost origin for development
                if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return uri.Host == "localhost" || 
                           uri.Host == "127.0.0.1" ||
                           uri.Host.StartsWith("192.168.") ||
                           uri.Host.StartsWith("10.") ||
                           uri.Host.StartsWith("172.");
                }
                return false;
            })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Important for SignalR
    });
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SMARTMOB PANTAREI API",
        Version = "v1",
        Description = "API for SMARTMOB PANTAREI system"
    });
    
    // Include XML comments for better API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SMARTMOB PANTAREI API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
// Configure static files for images
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Public")),
    RequestPath = "/images"
});

// Also expose the same Public folder under /api/images/public so frontend requests
// like GET /api/images/public/theimageName.jpg will resolve directly to files in Public/
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Public")),
    RequestPath = "/api/images/public",
    ServeUnknownFileTypes = true // allow serving files by extension; content-type provider will still apply for known types
});

// Use CORS - prefer the credentials-enabled policy for SignalR (frontend apps)
// SignalR requires AllowCredentials() when the client sends cookies or uses credentialed requests.
// Note: AllowAnyOrigin() cannot be combined with AllowCredentials(), so we use the more restrictive policy.
app.UseCors("AllowSpecific");

// Add authentication and authorization middleware (if needed)
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();
// Map SignalR Hub
app.MapHub<AcquisizioniHub>("/hubs/acquisizioni");

app.Run();
