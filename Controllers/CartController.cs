using AmazonCloneMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace AmazonCloneMVC.Controllers
{
    [Authorize]
    public class CartController : Controller
    {

        private readonly EcomDbContext _context;
        private readonly ILogger<CartController> _logger;
        private readonly IMemoryCache _cache;
        public CartController(EcomDbContext context, IMemoryCache cache, ILogger<CartController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }


        public IActionResult Index()
        {
            var userId = GetCurrentUserId();
            var userWithCart = _context.Users.Include(c => c.Cart).ThenInclude(cart => cart.Products)
                .FirstOrDefault(c => c.userId == userId);

            if (userWithCart == null)
            {
                return View();
            }

            var cart = userWithCart.Cart;


            ViewBag.Products = cart.Products;
            return View(cart.Products);
        }

        private int GetCurrentUserId()
        {
            if (_cache.TryGetValue<int>("UserId", out var userId))
            {
                return userId;
            }
            return 0;
        }
        
        public IActionResult AddToCart(int productId)
        {
            var userId = GetCurrentUserId();
            var user = _context.Users.Include(c => c.Cart).FirstOrDefault(c => c.userId == userId);

            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (user.Cart == null)
            {
                var newCart = new Cart();
                user.Cart = newCart;
                _context.Carts.Add(newCart);
                _context.SaveChanges();
            }

            var cart = user.Cart;
            var product = _context.Products.Find(productId);
            cart.Products.Add(product);
               
           
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        

        public IActionResult RemoveProd([Bind("productId")] int productId)
        {
            var userId = GetCurrentUserId();
            var user = _context.Users.Include(c => c.Cart).FirstOrDefault(c => c.userId == userId);

            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

       
            var cart = user.Cart;
            var product = _context.Products.Find(productId);
            cart.Products.Remove(product);


            _context.SaveChanges();

            return RedirectToAction(nameof(Index));

        }


    }
}
