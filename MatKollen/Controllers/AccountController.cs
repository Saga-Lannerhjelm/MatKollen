#nullable disable
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

            var fetchedUser = _accountRepository.GetUserCredentials(user, out string error);
            if (fetchedUser?.Username == null && error == "")
            {
                ModelState.AddModelError("wrongCredentials", "Fel användarnamn eller lösenord");
            }

            if (fetchedUser?.Username != null)
            {
                var hashedPassword = HashPassword(user.Password, fetchedUser.Salt);
                bool userIsValid = hashedPassword.SequenceEqual(fetchedUser.Password);
                if (!userIsValid && error == "")
                {
                    ModelState.AddModelError("wrongCredentials", "Fel användarnamn eller lösenord");
                }

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

                    return RedirectToAction("Index", "UserFoodItems");
                }
            }
            if (error != "")
            {
                TempData["error"] = "Fel: " + error;
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

            string base64Salt = GenerateSalt();
            user.Salt = base64Salt;
            
            var hashedPassword = HashPassword(user.Password, base64Salt);
            user.Password = hashedPassword;

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


        // Implementerad based on code example 
        // from https://www.thatsoftwaredude.com/content/6218/how-to-encrypt-passwords-using-sha-256-in-c-and-net
        private static string HashPassword(string password, string salt)
        {
            var passwordSalt = password + salt;
            SHA256 sha256 = SHA256.Create();
            byte[] hashPassword;
            UTF8Encoding objUtf8 = new UTF8Encoding();
            hashPassword = sha256.ComputeHash(objUtf8.GetBytes(passwordSalt));
            string hashPasswordString = Convert.ToBase64String(hashPassword);

            return hashPasswordString;
        }

        // Based on code example from https://medium.com/@imAkash25/hashing-and-salting-passwords-in-c-0ee223f07e20
        // and https://juldhais.net/secure-way-to-store-passwords-in-database-using-sha256-asp-net-core-898128d1c4ef
        private static string GenerateSalt()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] byteSalt = new byte[16];
            rng.GetBytes(byteSalt);
            string salt = Convert.ToBase64String(byteSalt);
            return salt;
        }

        public IActionResult Logout()
        {
            // To delete the cookie I had to create a new cookie with an expired expiration date
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