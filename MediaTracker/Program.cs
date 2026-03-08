using MediaTracker.Core.Infrastructure.Localization;
using MediaTracker.Core.Services;
using MediaTracker.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using MediaTracker.Core.Integrations.Spotify;
using MediaTracker.Core.Integrations.YouTube;
using MediaTracker.Core.Integrations.Apple;
using Hangfire;
using MediaTracker.Infrastructure.Hangfire;
using Hangfire.SQLite;
using MediaTracker.Data;

const string DEFAULT_CULTURE = "bg";

var builder = WebApplication.CreateBuilder(args);

#region Configure Localization

builder.Services.AddSingleton<IStringLocalizerFactory, ResourcesLocalizerFactory>();
builder.Services.AddSingleton<IStringLocalizer, ResourcesLocalizer>();
builder.Services.AddLocalization();

#endregion Configure Localization

builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

#region Configure Data Layer

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<IRepository, Repository>();

#endregion Configure Data Layer

#region Configure Hangfire

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage(connectionString));
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1; 
});

#endregion

#region Configure Authentication

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddErrorDescriber<MultilanguageIdentityErrorDescriber>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = ".MediaTracker.Identity";
    options.Cookie.Path = "/";
    options.Cookie.Domain = builder.Configuration["AuthenticationCookie:Domain"];
    options.ExpireTimeSpan = TimeSpan.FromMinutes(Convert.ToDouble(builder.Configuration["AuthenticationCookie:ExpirationTime"]));

    options.LoginPath = "/login";
    options.AccessDeniedPath = "/login";
    options.SlidingExpiration = true;
});

#endregion Configure Authentication

#region Configure Servies

builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IExploreService, ExploreService>();
builder.Services.AddScoped(x =>
{
    var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
    var factory = x.GetRequiredService<IUrlHelperFactory>();
    return factory.GetUrlHelper(actionContext);
});

builder.Services.Configure<SpotifySettings>(builder.Configuration.GetSection(SpotifySettings.SectionName));
builder.Services.AddHttpClient<ISpotifyService, SpotifyService>();

builder.Services.Configure<YouTubeSettings>(builder.Configuration.GetSection(YouTubeSettings.SectionName));
builder.Services.AddHttpClient<IYouTubeService, YouTubeService>();

builder.Services.AddHttpClient<IApplePodcastService, ApplePodcastService>();

builder.Services.AddScoped<MorningUpdatesJob>();

#endregion

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthentication().UseCookiePolicy();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorizationFilter()]
});
HangfireConfiguration.ConfigureRecurringJobs();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(DEFAULT_CULTURE),
    SupportedCultures = [new CultureInfo(DEFAULT_CULTURE)],
    SupportedUICultures = [new CultureInfo(DEFAULT_CULTURE)],
    RequestCultureProviders = [new CookieRequestCultureProvider()]
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();