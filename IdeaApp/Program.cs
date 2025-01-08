using IdeaApp.Data;
using IdeaApp.Models;
using IdeaApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Link DB Context to database
builder.Services.AddDbContext<IdeaappDbContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("IdeaApp"));
});

//Add Identity(for authentication and authorization) to our app
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<IdeaappDbContext>()
    .AddDefaultTokenProviders(); 

//configuration settings for identity
builder.Services.Configure<IdentityOptions>(options => 
{
    //Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    //User settings
    options.User.RequireUniqueEmail = true;

    //Sign in settings
    options.SignIn.RequireConfirmedEmail = true;
});

builder.Services.AddAuthorization();

builder.Services.ConfigureApplicationCookie(options => 
{
    options.Cookie.HttpOnly = true;
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Account/Login";
    options.SlidingExpiration = true;
});

//register the iemailsender service
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(options => 
{
    options.SendGridKey = Environment.GetEnvironmentVariable("SendGridAPIKey");
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=LandingPage}/{id?}")
    .WithStaticAssets();


app.Run();
