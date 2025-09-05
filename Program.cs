using DinkToPdf;
using DinkToPdf.Contracts;
using DinkToPdfAll;
using LittleArkFoundation.Areas.Admin.Hubs;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using LittleArkFoundation.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Build.Execution;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ReturnUrlParameter = "returnUrl";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;
        options.Cookie.IsEssential = true;
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    //options.IdleTimeout = TimeSpan.FromMinutes(10); // Extend session timeout if needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

LibraryLoader.Load();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<ConnectionService>();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddSignalR();

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(16969); // Change port as needed
//});

builder.Services.AddScoped<ApplicationDbContext>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("DefaultConnection");
    return new ApplicationDbContext(connectionString);
});


var app = builder.Build();

// assign hub context to static LoggingService
using (var scope = app.Services.CreateScope())
{
    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<LogsHub>>();
    LoggingService.HubContext = hubContext;
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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
app.UseSession();

app.MapHub<LogsHub>("/logsHub");
app.MapHub<UsersHub>("/usersHub");

app.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller}/{action}/{id?}"
    );

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
