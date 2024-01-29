using AmazonCloneMVC.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Security.Claims;

namespace AmazonCloneMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IMemoryCache _cache;
        private readonly EcomDbContext _context;
        public AuthController(EcomDbContext context, IMemoryCache cache, ILogger<AuthController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            if (string.IsNullOrEmpty(user.email) || string.IsNullOrEmpty(user.password))
            {
                ModelState.AddModelError(string.Empty, "Email and password are required.");
                return View();
            }
            var dbClient =
                _context.Users.FirstOrDefault(c => c.email == user.email && c.password == user.password);
            if (dbClient != null)
            {

                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.email),
                    new Claim("Role", "User")
                };
                ClaimsIdentity claimsIdentity =
                    new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                AuthenticationProperties properties = new AuthenticationProperties()
                {
                    AllowRefresh = true,
                    IsPersistent = true
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), properties);
                _cache.Set<int>("UserId", dbClient.userId);
                _logger.LogInformation("User authentifié");

                if (dbClient.Cart == null)
                {
                    var existingCart = _context.Carts.FirstOrDefault(c => c.UserId == dbClient.userId);

                    if (existingCart == null)
                    {
                        var newCart = new Cart();
                        dbClient.Cart = newCart;

                        try
                        {
                            _context.Carts.Add(newCart);
                            _context.SaveChanges();
                        }
                        catch (DbUpdateException ex)
                        {
                            _logger.LogError(ex, "Erreur lors de l'enregistrement du panier dans la base de données");
                            throw;
                        }
                    }
                    else
                    {
                        dbClient.Cart = existingCart;
                    }
                }
                return RedirectToAction("Index", "Home");
            }
         else ModelState.AddModelError(string.Empty, "Invalid username or password");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var role = User.FindFirst("Role")?.Value;

            if (role == "User")
            {
                _cache.Remove("UserId");
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
        public IActionResult Index()
    {
        return View();
    } 
    }

    



    
}
