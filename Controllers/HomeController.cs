using AmazonCloneMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace AmazonCloneMVC.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EcomDbContext _context;
        private readonly IMemoryCache _cache;

        public int ProductId { get; set; }
        public HomeController(ILogger<HomeController> logger, EcomDbContext context,IMemoryCache cache)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
        }

        public async Task<IActionResult> IndexAsync([Bind("SearchedProduct")] string searchedProduct, [Bind("SelectedCategory")] string selectedCategory)
        {
            IQueryable<Product> productsQuery = _context.Products.Include(p => p.Category);
            _logger.LogInformation("Product Retrieving List: {DT}", DateTime.UtcNow.ToLongTimeString());

            ViewData["SelectedCategory"] = selectedCategory;

            if (!int.TryParse(selectedCategory, out int categoryId))
            {
                categoryId = 0; // Default category ID if parsing fails
            }

            if (!string.IsNullOrEmpty(searchedProduct))
            {
                // Filter by product name
                productsQuery = productsQuery.Where(p => EF.Functions.Like(p.Name, $"%{searchedProduct}%"));
                _logger.LogInformation("Product Retrieving List with searching string: {DT}", DateTime.UtcNow.ToLongTimeString());

            }

            if (categoryId != 0)
            {
                // Filter by category ID if specified
                productsQuery = productsQuery.Where(p => p.CategoryID == categoryId);
                _logger.LogInformation("Product Retrieving List with a specific category string: {DT}", DateTime.UtcNow.ToLongTimeString());

            }

            List<Product> Products = await productsQuery.ToListAsync();

            // Load categories if needed
            List<Category> Categories = await _context.Categories.ToListAsync();
            _logger.LogInformation("Product Retrieving List of category string: {DT}", DateTime.UtcNow.ToLongTimeString());

            ViewBag.Products = Products;
            ViewBag.Categories = Categories;

            return View(Products);
        }
        private int GetCurrentUserId()
        {
            if (_cache.TryGetValue<int>("UserId", out var userId))
            {
                return userId;
            }
            return 0;
        }
        public IActionResult AddToCart([Bind("productId")] int productId)
        {
            var userId = GetCurrentUserId();
            var user = _context.Users.Include(c => c.Cart).FirstOrDefault(c => c.userId == userId);

            if (user == null)
            {
                return View();
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

            return RedirectToAction("Index");
        }



       
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

