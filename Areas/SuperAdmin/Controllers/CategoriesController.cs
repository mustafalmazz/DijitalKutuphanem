using BookManagementApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookManagementApp.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class CategoriesController : Controller
    {
        private readonly MyDbContext _context;
        public CategoriesController(MyDbContext context)
        {
            _context = context;
        }
        public IActionResult List()
        {
            var list = _context.Categories.Include(c => c.Books).OrderBy(c => c.CategoryName).ToList();
            return View(list);
        }
        public IActionResult DeleteConfirm(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        [HttpPost]
        public IActionResult Delete(Category category)
        {
            var existingCategory = _context.Categories.FirstOrDefault(c=>c.Id == category.Id);
            if (existingCategory == null)
            {
                return NotFound();
            }
            _context.Categories.Remove(existingCategory);
            _context.SaveChanges();

            return RedirectToAction("List");

        }
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "Kategori adı boş olamaz.");
                return View(category);
            }

            category.CategoryName = category.CategoryName.Trim();

            // Kategoriler global olduğu için aynı isim iki kez bulunmamalı
            if (_context.Categories.Any(c => c.CategoryName == category.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "Bu isimde bir kategori zaten var.");
                return View(category);
            }

            _context.Categories.Add(category);
            _context.SaveChanges();
            return RedirectToAction("List");
        }

        public IActionResult Edit(int id)
        {
            var cat = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (cat == null)
            {
                return NotFound();
            }

            return View(cat);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            var existingCategory = _context.Categories.FirstOrDefault(c => c.Id == category.Id);
            if (existingCategory == null)
            {
                return NotFound();
            }
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "Kategori adı boş olamaz.");
                return View(category);
            }

            var trimmed = category.CategoryName.Trim();
            if (_context.Categories.Any(c => c.CategoryName == trimmed && c.Id != category.Id))
            {
                ModelState.AddModelError("CategoryName", "Bu isimde bir kategori zaten var.");
                return View(category);
            }

            existingCategory.CategoryName = trimmed;
            _context.SaveChanges();
            return RedirectToAction("List");
        }
    }
}
