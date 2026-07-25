using GymManagementSystem.Data;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.Controllers;

[Authorize]  // Both Admin and Operator can access
public class MembersController : Controller
{
    private readonly ApplicationDbContext _db;
    public MembersController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var members = await _db.Members
            .Include(m => m.Trainer)
            .Include(m => m.Subscriptions)
            .ToListAsync();
        return View(members);
    }

    public IActionResult Create()
    {
        ViewBag.Trainers = new SelectList(_db.Trainers.Where(t => t.IsActive), "Id", "FullName");
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member member)
    {
        if (ModelState.IsValid)
        {
            _db.Members.Add(member);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Trainers = new SelectList(_db.Trainers.Where(t => t.IsActive), "Id", "FullName");
        return View(member);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();
        ViewBag.Trainers = new SelectList(_db.Trainers.Where(t => t.IsActive), "Id", "FullName");
        return View(member);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Member member)
    {
        if (ModelState.IsValid)
        {
            _db.Members.Update(member);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(member);
    }

    public async Task<IActionResult> Details(int id)
    {
        var member = await _db.Members
            .Include(m => m.Trainer)
            .Include(m => m.Subscriptions)
            .Include(m => m.Attendances)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (member == null) return NotFound();
        return View(member);
    }

    [Authorize(Roles = "Admin")]  // Only Admin can delete
    public async Task<IActionResult> Delete(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member != null)
        {
            _db.Members.Remove(member);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}