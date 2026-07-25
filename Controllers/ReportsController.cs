using GymManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.Controllers;

[Authorize(Roles = "Admin")]   // ← ONLY ADMIN CAN ACCESS
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    public ReportsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var thisMonth = new DateTime(today.Year, today.Month, 1);

        var vm = new
        {
            TotalMembers = await _db.Members.CountAsync(),
            ActiveMembers = await _db.Members.CountAsync(m => m.IsActive),
            TotalTrainers = await _db.Trainers.CountAsync(),
            TodayAttendance = await _db.Attendances.CountAsync(a => a.CheckIn.Date == today),
            MonthlyRevenue = await _db.Subscriptions
                                    .Where(s => s.CreatedAt >= thisMonth && s.IsPaid)
                                    .SumAsync(s => (decimal?)s.Amount) ?? 0,
            ExpiringThisWeek = await _db.Subscriptions
                                    .Include(s => s.Member)
                                    .Where(s => s.EndDate >= today && s.EndDate <= today.AddDays(7))
                                    .ToListAsync(),
            RecentAttendance = await _db.Attendances
                                    .Include(a => a.Member)
                                    .OrderByDescending(a => a.CheckIn)
                                    .Take(10)
                                    .ToListAsync()
        };

        return View(vm);
    }

    public async Task<IActionResult> Revenue(int? year, int? month)
    {
        year ??= DateTime.Today.Year;
        month ??= DateTime.Today.Month;

        var data = await _db.Subscriptions
            .Where(s => s.CreatedAt.Year == year && s.IsPaid)
            .GroupBy(s => s.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Total = g.Sum(s => s.Amount) })
            .ToListAsync();

        ViewBag.Year = year;
        ViewBag.MonthlyData = data;
        return View();
    }
}