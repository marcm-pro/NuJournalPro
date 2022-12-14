using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NuJournalPro.Data;
using NuJournalPro.Models;
using NuJournalPro.Models.ViewModels;
using NuJournalPro.Services;
using NuJournalPro.Services.Identity;
using NuJournalPro.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
//var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

// Add services to the container.
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString = builder.Configuration.GetSection("pgsqlSettings")["pgsqlConnection"];

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
//builder.Services.AddDefaultIdentity<NuJournalUser>(options => options.SignIn.RequireConfirmedAccount = true)
//                .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentity<NuJournalUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddDefaultUI()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

// Load custom configurations.
builder.Services.Configure<OwnerSettings>(builder.Configuration.GetSection("OwnerSettings"));
builder.Services.Configure<DefaultUserSettings>(builder.Configuration.GetSection("DefaultUserSettings"));
builder.Services.Configure<DefaultGraphics>(builder.Configuration.GetSection("DefaultGraphics"));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<ContactUsSettings>(builder.Configuration.GetSection("ContactUsSettings"));

// Register custom services.
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<ICompressionService, CompressionService>();
builder.Services.AddSingleton<IImageService, ImageService>();
builder.Services.AddScoped<ISlugService, BasicSlugService>();
builder.Services.AddScoped<IEmailSender, EmailService>();
builder.Services.AddScoped<IContactEmailSender, ContactEmailSender>();
builder.Services.AddTransient<DataService>();


var app = builder.Build();

// Get the database update with the latest migrations and create an Administrator user if it doesn't already exist.
var dataService = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataService>();
await dataService.ManageDataAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
