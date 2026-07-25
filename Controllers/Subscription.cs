using GymManagementSystem.Data;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.Controllers;

[Authorize]
public class SubscriptionsController : Controller
{
    private readonly ApplicationDbContext _db;
    public SubscriptionsController(ApplicationDbContext db) => _db = db;

    // ── INDEX ─────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var subscriptions = await _db.Subscriptions
            .Include(s => s.Member)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
        return View(subscriptions);
    }

    // ── CREATE GET ────────────────────────────────────
    public IActionResult Create(int? memberId)
    {
        ViewBag.Members = new SelectList(
            _db.Members.Where(m => m.IsActive).OrderBy(m => m.FullName),
            "Id", "FullName", memberId);

        // Pre-fill dates
        var model = new Subscription
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1)
        };

        // Pre-select member if coming from Member Details
        if (memberId.HasValue)
            model.MemberId = memberId.Value;

        return View(model);
    }

    // ── CREATE POST ───────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Subscription subscription)
    {
        if (ModelState.IsValid)
        {
            subscription.CreatedAt = DateTime.Now;
            _db.Subscriptions.Add(subscription);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Subscription added successfully!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Members = new SelectList(
            _db.Members.Where(m => m.IsActive).OrderBy(m => m.FullName),
            "Id", "FullName", subscription.MemberId);

        return View(subscription);
    }

    // ── EDIT GET ──────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var subscription = await _db.Subscriptions
            .Include(s => s.Member)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subscription == null) return NotFound();

        ViewBag.Members = new SelectList(
            _db.Members.Where(m => m.IsActive).OrderBy(m => m.FullName),
            "Id", "FullName", subscription.MemberId);

        return View(subscription);
    }

    // ── EDIT POST ─────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Subscription subscription)
    {
        if (ModelState.IsValid)
        {
            _db.Subscriptions.Update(subscription);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Subscription updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Members = new SelectList(
            _db.Members.Where(m => m.IsActive).OrderBy(m => m.FullName),
            "Id", "FullName", subscription.MemberId);

        return View(subscription);
    }

    // ── MARK AS PAID (Admin Only) ─────────────────────
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> MarkPaid(int id)
    {
        var subscription = await _db.Subscriptions.FindAsync(id);
        if (subscription != null)
        {
            subscription.IsPaid = true;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Subscription marked as paid!";
        }
        return RedirectToAction(nameof(Index));
    }

    // ── DELETE (Admin Only) ───────────────────────────
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var subscription = await _db.Subscriptions.FindAsync(id);
        if (subscription != null)
        {
            _db.Subscriptions.Remove(subscription);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Subscription deleted successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    // ── AUTO CALCULATE END DATE (AJAX) ────────────────
    [HttpGet]
    public IActionResult GetEndDate(DateTime startDate, SubscriptionPlan plan)
    {
        var endDate = plan switch
        {
            SubscriptionPlan.Monthly => startDate.AddMonths(1),
            SubscriptionPlan.Quarterly => startDate.AddMonths(3),
            SubscriptionPlan.SemiAnnual => startDate.AddMonths(6),
            SubscriptionPlan.Annual => startDate.AddYears(1),
            _ => startDate.AddMonths(1)
        };
        return Json(endDate.ToString("yyyy-MM-dd"));
    }
}