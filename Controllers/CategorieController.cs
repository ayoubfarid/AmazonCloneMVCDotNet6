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
    public class CategorieController : Controller
    {
        private readonly EcomDbContext _context;
        private readonly ILogger<CategorieController> _logger;

        public CategorieController(EcomDbContext context, ILogger<CategorieController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Categorie
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Category Retrieving List : {DT}", DateTime.UtcNow.ToLongTimeString());

            return _context.Categories != null ? 
                          View(await _context.Categories.ToListAsync()) :
                          Problem("Entity set 'EcomDbContext.Categories'  is null.");
        }

        // GET: Categorie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            _logger.LogInformation("Category Retrieving Details : {DT}", DateTime.UtcNow.ToLongTimeString());

            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryID == id);
            if (category == null)
            {
                _logger.LogWarning("Category Not Found Retrieving  : {DT}", DateTime.UtcNow.ToLongTimeString());

                return NotFound();
            }

            return View(category);
        }

        // GET: Categorie/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categorie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryID,Name,Description")] Category category)
        {
            _logger.LogInformation("Category Creation  : {DT}", DateTime.UtcNow.ToLongTimeString());

            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Category Created  : {DT}", DateTime.UtcNow.ToLongTimeString());

                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categorie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            _logger.LogInformation("Category Edit  : {DT}", DateTime.UtcNow.ToLongTimeString());

            if (id == null || _context.Categories == null)
            {
                _logger.LogWarning("Category Not Found  : {DT}", DateTime.UtcNow.ToLongTimeString());

                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            _logger.LogWarning("Category Found  : {DT}", DateTime.UtcNow.ToLongTimeString());

            return View(category);
        }

        // POST: Categorie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryID,Name,Description")] Category category)
        {
            _logger.LogInformation("Category Edit  : {DT}", DateTime.UtcNow.ToLongTimeString());

            if (id != category.CategoryID)
            {
                _logger.LogWarning("Category Id Mismatching  : {DT}", DateTime.UtcNow.ToLongTimeString());

                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Category Edited  : {DT}", DateTime.UtcNow.ToLongTimeString());

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryID))
                    {
                        _logger.LogWarning("Category Not Found  : {DT}", DateTime.UtcNow.ToLongTimeString());

                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categorie/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            _logger.LogInformation("Category Delete  : {DT}", DateTime.UtcNow.ToLongTimeString());


            if (id == null || _context.Categories == null)
            {
                _logger.LogWarning("Category Not Found  : {DT}", DateTime.UtcNow.ToLongTimeString());

                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryID == id);
            if (category == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Category Deleted  : {DT}", DateTime.UtcNow.ToLongTimeString());

            return View(category);
        }

        // POST: Categorie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'EcomDbContext.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
          return (_context.Categories?.Any(e => e.CategoryID == id)).GetValueOrDefault();
        }
    }
}
