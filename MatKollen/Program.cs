using System.Net;
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

// Register repositories
builder.Services.AddScoped<FoodRepository>();
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<RecipeRepository>();
builder.Services.AddScoped<GroceryListRepository>();
builder.Services.AddScoped<GetListsRepository>();


builder.Services.AddScoped<FoodService>();

// Added to handle sessions. Retrieved from https://www.canvas.umu.se/courses/15315/pages/undervisningsfilmer-asp-dot-net-mvc 4th september 2024
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

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

// Code taken from https://learn.microsoft.com/en-us/answers/questions/1114785/how-to-make-login-path-without-using-cookie-in-asp
app.UseStatusCodePages(async context => {  
    var request = context.HttpContext.Request;  
    var response = context.HttpContext.Response;  

    if (response.StatusCode == (int)HttpStatusCode.Unauthorized)  
    {  
        response.Redirect("/Account/Login");
    }  
}); 

app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=FoodList}/{action=Index}/{id?}");

app.Run();
