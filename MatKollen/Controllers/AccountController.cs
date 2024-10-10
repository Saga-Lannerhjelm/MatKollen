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
    // [ApiController]
    // [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private IConfiguration _config;

        public AccountController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(User user)
        {
            IActionResult response = Unauthorized();
            var AccountRep = new AccountRepository();

            var hashedPassword = CalculateSHA256(user.Password);
            var fetchedUser = AccountRep.GetUserCredentials(user, out string error);

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
                        expires: DateTime.Now.AddMinutes(5),
                        signingCredentials: credentials);

                    var tokenResult = new JwtSecurityTokenHandler().WriteToken(token);

                    return Ok(tokenResult);
                }
            }
            if (error != "")
            {
                TempData["unsuccessful"] = "Fel: " + error;
                View(user);
            }


            return response;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            var hashedPassword = CalculateSHA256(user.Password);
            user.PasswordHashed = hashedPassword;

            var accountRep = new AccountRepository();

            var rowsAffectd = accountRep.InertUser(user, out string error);

            if (rowsAffectd != 0)
            {
                return RedirectToAction("Index", "Home");
            }

            if (error != "")
            {
                TempData["unsuccessful"] = "Fel: " + error;
            }

            TempData["unsuccessful"] = "Ingen användare skapades";
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

    }
}