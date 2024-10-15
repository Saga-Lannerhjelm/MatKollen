using System.Text;
using MatKollen.Controllers;
using MatKollen.Controllers.Repositories;
using MatKollen.DAL.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwtIssuer = builder.Configuration.GetSection("JwtSettings:Issuer").Get<string>();
var jwtKey = builder.Configuration.GetSection("JwtSettings:Key").Get<string>();
var jwtAudience = builder.Configuration.GetSection("JwtSettings:Audience").Get<string>();

builder.Services
.AddAuthentication(a => 
{
    a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    a.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
// .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
//     // options => builder.Configuration.Bind("CookieSettings", options)
//     options => 
//     {
//         options.Cookie.Name = "Jwt-cookie";
//         options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
//         options.SlidingExpiration = true;
//     }
// )
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = jwtIssuer,
         ValidAudience = jwtAudience,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
    options.SaveToken = true;

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context => 
        {
            if (context.Request.Cookies.ContainsKey("Jwt-cookie"))
            {
                context.Token = context.Request.Cookies["Jwt-cookie"];
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});


// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// Register resositories
builder.Services.AddScoped<FoodRepository>();
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<RecipeRepository>();


builder.Services.AddScoped<FoodService>();


var app = builder.Build();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
