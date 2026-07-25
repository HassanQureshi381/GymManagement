using GymManagementSystem.Data;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GymManagementSystem.Controllers;

[Authorize]
public class AttendanceController : Controller
{
    private readonly ApplicationDbContext _db;
    public AttendanceController(ApplicationDbContext db) => _db = db;

    // ── INDEX ─────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var records = await _db.Attendances
            .Include(a => a.Member)
            .Where(a => a.CheckIn.Date == today)
            .OrderByDescending(a => a.CheckIn)
            .ToListAsync();
        return View(records);
    }

    // ── CHECK IN GET ──────────────────────────────────
    public IActionResult CheckIn()
    {
        var members = _db.Members
            .Where(m => m.IsActive)
            .OrderBy(m => m.FullName)
            .Select(m => new {
                id = m.Id,
                fullName = m.FullName,
                email = m.Email,
                phone = m.Phone
            })
            .ToList();

        // IDs of members already checked in today
        var todayCheckedInIds = _db.Attendances
            .Where(a => a.CheckIn.Date == DateTime.Today
                     && a.CheckOut == null)
            .Select(a => a.MemberId)
            .ToList();

        ViewBag.MembersJson = JsonSerializer.Serialize(members);
        ViewBag.TodayCheckedInIds = JsonSerializer.Serialize(todayCheckedInIds);

        return View();
    }

    // ── CHECK IN POST ─────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(int memberId, string? notes)
    {
        if (memberId == 0)
        {
            TempData["Error"] = "Please select a member.";
            return RedirectToAction(nameof(CheckIn));
        }

        // Check if already checked in today without checkout
        var existing = await _db.Attendances
            .Where(a => a.MemberId == memberId
                     && a.CheckIn.Date == DateTime.Today
                     && a.CheckOut == null)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            TempData["Error"] = "Member is already checked in.";
            return RedirectToAction(nameof(Index));
        }

        // Get member name for success message
        var member = await _db.Members.FindAsync(memberId);

        _db.Attendances.Add(new Attendance
        {
            MemberId = memberId,
            CheckIn = DateTime.Now,
            Notes = notes
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"✅ {member?.FullName} checked in successfully!";
        return RedirectToAction(nameof(Index));
    }

    // ── CHECK OUT ─────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> CheckOut(int id)
    {
        var record = await _db.Attendances
            .Include(a => a.Member)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (record != null && record.CheckOut == null)
        {
            record.CheckOut = DateTime.Now;
            await _db.SaveChangesAsync();
            TempData["Success"] =
                $"✅ {record.Member?.FullName} checked out successfully!";
        }

        return RedirectToAction(nameof(Index));
    }
}