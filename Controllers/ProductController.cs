using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AmazonCloneMVC.Models;
using Microsoft.AspNetCore.Authorization;

namespace AmazonCloneMVC.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly EcomDbContext _context;
        private readonly ILogger<ProductController> _logger;
        
        public ProductController(ILogger<ProductController> logger, EcomDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Product
        public async Task<IActionResult> IndexAsync([Bind("SearchedProduct")] string searchedProduct, [Bind("SelectedCategory")] string selectedCategory)
        {
            IQueryable<Product> productsQuery = _context.Products.Include(p => p.Category);
            _logger.LogInformation("Product Retrieving List : {DT}", DateTime.UtcNow.ToLongTimeString());

            ViewData["SelectedCategory"] = selectedCategory;

            if (!int.TryParse(selectedCategory, out int categoryId))
            {
                categoryId = 0; // Default category ID if parsing fails
            }

            if (!string.IsNullOrEmpty(searchedProduct))
            {
                // Filter by product name
                productsQuery = productsQuery.Where(p => EF.Functions.Like(p.Name, $"%{searchedProduct}%"));

                _logger.LogInformation("Product Retrieving List by specified string : {DT}", DateTime.UtcNow.ToLongTimeString());

            }

            if (categoryId != 0)
            {
                // Filter by category ID if specified
                productsQuery = productsQuery.Where(p => p.CategoryID == categoryId);
                _logger.LogInformation("Product Retrieving List by specified category : {DT}", DateTime.UtcNow.ToLongTimeString());

            }

            List<Product> Products = await productsQuery.ToListAsync();

            // Load categories if needed
            List<Category> Categories = await _context.Categories.ToListAsync();
           ViewBag.Products = Products;
            ViewBag.Categories = Categories;
            _logger.LogInformation("Product Retrieving List Done : {DT}", DateTime.UtcNow.ToLongTimeString());


            return View(ViewBag.Products);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            _logger.LogInformation("Product Retrieving Details : {DT}", DateTime.UtcNow.ToLongTimeString());

            if (id == null || _context.Products == null)
            {
                _logger.LogWarning("Product  Details : {DT}", DateTime.UtcNow.ToLongTimeString());

                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                _logger.LogWarning("Product Not Found Details : {DT}", DateTime.UtcNow.ToLongTimeString());

                return NotFound();
            }
            _logger.LogInformation("Product Retrivied Details  : {DT}", DateTime.UtcNow.ToLongTimeString());

            return View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryID");
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductID,Name,Description,Price,Quantity,Image,ImageFile,CategoryID")] Product product)
        {
            _logger.LogInformation("Product Create  : {DT}", DateTime.UtcNow.ToLongTimeString());

            if (!ModelState.IsValid || _context.Products == null || product == null)
            {
                _logger.LogWarning("Product Not Valid  : {DT}", DateTime.UtcNow.ToLongTimeString());

                return View(product);
            }
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            Console.WriteLine("exec 2");
            if (ModelState.IsValid)
            {// Handle the uploaded image
                if (product.ImageFile != null && product.ImageFile.Length > 0)
                {
                    _logger.LogWarning("Product Image path generate  : {DT}", DateTime.UtcNow.ToLongTimeString());

                    // Generate a unique file name (you can use GUIDs, timestamps, etc.)
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName;

                    // Combine the unique file name with the path to the "images" folder
                    var imagePath = Path.Combine("images", uniqueFileName);

                    // Save the image to the wwwroot/images folder
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(stream);
                    }

                    product.Image = "/" + imagePath; // Update the ImagePath property with the relative path
                }
                _context.Add(product);
                await _context.SaveChangesAsync();
                _logger.LogWarning("Product Stored  : {DT}", DateTime.UtcNow.ToLongTimeString());

                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryID", product.CategoryID);
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryID", product.CategoryID);
            return View(product);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,Name,Description,Price,Quantity,Image,CategoryID")] Product product)
        {
            _logger.LogWarning("Product Edit  : {DT}", DateTime.UtcNow.ToLongTimeString());

            if (id != product.ProductID)
            {
                _logger.LogWarning("Product Id Mismatching  : {DT}", DateTime.UtcNow.ToLongTimeString());

                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductID))
                    {
                        _logger.LogWarning("Product Not Found  : {DT}", DateTime.UtcNow.ToLongTimeString());

                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError("Product Saving issues  : {DT}", DateTime.UtcNow.ToLongTimeString());

                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryID", product.CategoryID);
            return View(product);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            _logger.LogInformation("Product Delete  : {DT}", DateTime.UtcNow.ToLongTimeString());

            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                _logger.LogWarning("Product Not Found  : {DT}", DateTime.UtcNow.ToLongTimeString());

                return NotFound();
            }
            _logger.LogInformation("Product Deleted  : {DT}", DateTime.UtcNow.ToLongTimeString());

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'EcomDbContext.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
          return (_context.Products?.Any(e => e.ProductID == id)).GetValueOrDefault();
        }
    }
}
