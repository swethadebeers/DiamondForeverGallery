using DiamondForeverGallery.Data;
using DiamondForeverGallery.Services;
using Microsoft.EntityFrameworkCore;

// Step 1: Create the builder. It loads settings (appsettings.json) and sets up the services container.
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Registers PhotoStorageService as a singleton: one instance is shared by the whole app.
builder.Services.AddSingleton<PhotoStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Redirects http:// requests to https:// for security.
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
// Connects each .cshtml page in the Pages folder to a URL.
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
