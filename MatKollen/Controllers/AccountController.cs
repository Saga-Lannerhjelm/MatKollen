using System.IdentityModel.Tokens;
using System.Text;
using MatKollen.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using MatKollen.DAL.Repositories;
using System.Security.Claims;

namespace MatKollen.Controllers
{
    public class AccountController : Controller
    {
        private IConfiguration _config;
        private readonly AccountRepository _accountRepository;

        public AccountController(IConfiguration config, AccountRepository accountRepository)
        {
            _config = config;
            _accountRepository = accountRepository;
        }

         [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            var expirationTime = 2;

            var hashedPassword = CalculateSHA256(user.Password);
            var fetchedUser = _accountRepository.GetUserCredentials(user, out string error);

            if (fetchedUser != null)
            {
                bool userIsValid = hashedPassword.SequenceEqual(fetchedUser.PasswordHashed);

                if (userIsValid)
                {
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                    var tokenClaims = new List<Claim>
                    {
                        new("id", fetchedUser.Id.ToString()),
                        new("name", fetchedUser.Username)
                    };

                    var token = new JwtSecurityToken(
                        issuer: _config["JwtSettings:Issuer"],
                        audience: _config["JwtSettings:Audience"],
                        claims: tokenClaims,
                        expires: DateTime.Now.AddHours(expirationTime),
                        signingCredentials: credentials);

                    var tokenResult = new JwtSecurityTokenHandler().WriteToken(token);

                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.Now.AddHours(expirationTime),
                        Secure = true,
                        HttpOnly = true,
                    };
                    
                    Response.Cookies.Append("Jwt-cookie", tokenResult, cookieOptions);

                    return RedirectToAction("Index", "Home");
                }
            }
            if (error != "")
            {
                TempData["error"] = "Fel: " + error;
            } else
            {
                TempData["error"] = "Fel användarnamn eller lösenord";
            }

            return View(user);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (user.Email == null)
            {
                ModelState.AddModelError(nameof(user.Email), "Fältet kan inte vara tomt");
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }
            
            var hashedPassword = CalculateSHA256(user.Password);
            user.PasswordHashed = hashedPassword;

            var rowsAffectd = _accountRepository.InsertUser(user, out string error);

            if (rowsAffectd != 0)
            {
                return RedirectToAction("Login", "Account");
            }

            TempData["error"] = "Ingen användare skapades";
            if (error != "")
            {
                TempData["error"] = "Fel: " + error;
            }
            return View();
        }


        // Implementerad baserat på kodexempel 
        // från https://www.thatsoftwaredude.com/content/6218/how-to-encrypt-passwords-using-sha-256-in-c-and-net
        private byte[] CalculateSHA256(string password)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] hashValue;
            UTF8Encoding objUtf8 = new UTF8Encoding();
            hashValue = sha256.ComputeHash(objUtf8.GetBytes(password));

            return hashValue;
        }

        public IActionResult Logout()
        {
            // To delete the cookie I used had to create a new cookie with an expired expiration date
            if (Request.Cookies["Jwt-cookie"] != null) {
                
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Secure = true,
                    HttpOnly = true,
                };
                    
                    Response.Cookies.Append("Jwt-cookie", "", cookieOptions);
            }
            return View("Login");
        }

    }
}