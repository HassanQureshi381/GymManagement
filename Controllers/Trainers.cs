using GymManagementSystem.Data;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.Controllers;

[Authorize]
public class TrainersController : Controller
{
    private readonly ApplicationDbContext _db;
    public TrainersController(ApplicationDbContext db) => _db = db;

    // ── INDEX ─────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var trainers = await _db.Trainers
            .Include(t => t.Members)
            .ToListAsync();
        return View(trainers);
    }

    // ── CREATE GET ────────────────────────────────────
    public IActionResult Create() => View();

    // ── CREATE POST ───────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Trainer trainer)
    {
        if (ModelState.IsValid)
        {
            _db.Trainers.Add(trainer);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Trainer added successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(trainer);
    }

    // ── EDIT GET ──────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var trainer = await _db.Trainers.FindAsync(id);
        if (trainer == null) return NotFound();
        return View(trainer);
    }

    // ── EDIT POST ─────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Trainer trainer)
    {
        if (ModelState.IsValid)
        {
            _db.Trainers.Update(trainer);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Trainer updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(trainer);
    }

    // ── DETAILS ───────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var trainer = await _db.Trainers
            .Include(t => t.Members)
                .ThenInclude(m => m.Subscriptions) // ← load subscriptions too
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trainer == null) return NotFound();
        return View(trainer);
    }

    // ── DELETE (Admin Only) ───────────────────────────
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var trainer = await _db.Trainers.FindAsync(id);
        if (trainer != null)
        {
            var members = await _db.Members
                .Where(m => m.TrainerId == id)
                .ToListAsync();

            foreach (var member in members)
                member.TrainerId = null;

            _db.Trainers.Remove(trainer);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Trainer deleted successfully!";
        }
        return RedirectToAction(nameof(Index));
    }
}