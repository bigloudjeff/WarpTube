using Microsoft.EntityFrameworkCore;
using WarpTube.Web.Components;
using WarpTube.Shared.Services;
using WarpTube.Web.Services;
using WarpTube.Shared.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure Entity Framework and SQLite
builder.Services.AddDbContextFactory<WarpTubeDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=warptube.db",
        b => b.MigrationsAssembly("WarpTube.Web"))
);

// Add device-specific services used by the WarpTube.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// Register YouTube download service
builder.Services.AddScoped<IYouTubeDownloadService, YouTubeDownloadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(WarpTube.Shared._Imports).Assembly);

app.Run();
