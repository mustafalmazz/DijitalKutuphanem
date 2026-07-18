using BookManagementApp.Areas.Admin.Models;
using BookManagementApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookManagementApp.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class UsersController : Controller
    {
        private readonly MyDbContext _context;
        public UsersController(MyDbContext context)
        {
            _context = context;
        }
        public IActionResult Search(string q)
        {
            var users = _context.Users.Where(u => u.UserName.Contains(q) || u.Email.Contains(q)).ToList();
            return View("List", users);
        }
        public IActionResult Add()
        {
            var roles = new List<string> { "User", "SuperAdmin" };
            ViewBag.Roles = new SelectList(roles, "User");
            return View();
        }
        [HttpPost]
        public IActionResult Add(User model)
        {
            if (_context.Users.Any(u => u.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", "Bu kullanıcı adı zaten sistemde kayıtlı.");
                return View(model);
            }

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                return View(model);
            }
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(model.PasswordHash))
                {
                    model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
                }
                _context.Users.Add(model);
                _context.SaveChanges();
                return RedirectToAction("List");
            }
            return View(model);
        }
        public IActionResult List()
        {
            var userList = _context.Users.Include(u => u.Books).ToList();


            return View(userList);
        }
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = _context.Users.Include(u => u.Books).FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = new List<string> { "User", "SuperAdmin" };
            ViewBag.Roles = new SelectList(roles, user.Role); ;
            return View(user);
        }
        [HttpPost]
        public IActionResult Edit(User model)
        {
            if (model == null)
            {
                return NotFound();
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == model.Id);
            if (user == null)
            {
                return NotFound();
            }
            user.UserName = model.UserName;
            user.Role = model.Role;
            user.Email = model.Email;
            //user.PasswordHash = model.PasswordHash;
            _context.SaveChanges();

            return RedirectToAction("List");
        }
   
        public async Task<IActionResult> DeleteConfirm(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // --- BİLGELİK TAŞI EKLE / ÇIKAR ---
        // operation: "add" | "subtract"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustStones(int id, int amount, string operation)
        {
            if (amount <= 0)
            {
                TempData["StoneError"] = "Miktar 0'dan büyük olmalı.";
                return RedirectToAction(nameof(List));
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (operation == "add")
            {
                // EarnStones hem bakiyeyi hem toplam kazancı artırır; taş kazandırmanın
                // tek doğru yolu budur (bkz. User.EarnStones).
                user.EarnStones(amount);
                TempData["StoneMessage"] = $"{user.UserName} kullanıcısına {amount} taş eklendi. Yeni bakiye: {user.WisdomStones}";
            }
            else if (operation == "subtract")
            {
                int actual = Math.Min(amount, user.WisdomStones); // bakiye eksiye düşmesin
                user.WisdomStones -= actual;

                // TotalStonesEarned bilerek düşürülmüyor: kazanılmış ilerleme geri alınmaz.
                // Aksi halde kullanıcı hak ettiği Bilgelik başarımlarını kaybederdi.
                TempData["StoneMessage"] = actual < amount
                    ? $"{user.UserName} kullanıcısının bakiyesi {actual} taş düşürüldü (bakiyesi yetersizdi). Yeni bakiye: {user.WisdomStones}"
                    : $"{user.UserName} kullanıcısından {actual} taş çıkarıldı. Yeni bakiye: {user.WisdomStones}";
            }
            else
            {
                TempData["StoneError"] = "Geçersiz işlem.";
                return RedirectToAction(nameof(List));
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existingUser = await _context.Users.FindAsync(id);

            if (existingUser == null)
            {
                return NotFound();
            }

            _context.Users.Remove(existingUser);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(List));
        }
    }
}
