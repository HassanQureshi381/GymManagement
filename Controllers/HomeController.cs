using GymManagementSystem.Data;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;

        var vm = new DashboardViewModel
        {
            TotalMembers = await _db.Members.CountAsync(),
            ActiveMembers = await _db.Members.CountAsync(m => m.IsActive),
            TotalTrainers = await _db.Trainers.CountAsync(),
            ActiveTrainers = await _db.Trainers.CountAsync(t => t.IsActive),
            TodayCheckIns = await _db.Attendances
                                       .CountAsync(a => a.CheckIn.Date == today),
            CurrentlyInGym = await _db.Attendances
                                       .CountAsync(a => a.CheckIn.Date == today
                                                     && a.CheckOut == null),
            ActiveSubscriptions = await _db.Subscriptions
                                       .CountAsync(s => s.EndDate >= today
                                                     && s.StartDate <= today),
            UnpaidSubscriptions = await _db.Subscriptions
                                       .CountAsync(s => !s.IsPaid),
            MonthlyRevenue = await _db.Subscriptions
                                       .Where(s => s.CreatedAt.Month == today.Month
                                                && s.CreatedAt.Year == today.Year
                                                && s.IsPaid)
                                       .SumAsync(s => (decimal?)s.Amount) ?? 0,
            RecentMembers = await _db.Members
                                       .OrderByDescending(m => m.JoinDate)
                                       .Take(5)
                                       .ToListAsync(),
            TodayAttendances = await _db.Attendances
                                       .Include(a => a.Member)
                                       .Where(a => a.CheckIn.Date == today)
                                       .OrderByDescending(a => a.CheckIn)
                                       .Take(5)
                                       .ToListAsync(),
            ExpiringThisWeek = await _db.Subscriptions
                                       .Include(s => s.Member)
                                       .Where(s => s.EndDate >= today
                                                && s.EndDate <= today.AddDays(7))
                                       .ToListAsync()
        };

        return View(vm);
    }
}