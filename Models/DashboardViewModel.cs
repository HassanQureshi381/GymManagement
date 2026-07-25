namespace GymManagementSystem.Models;

public class DashboardViewModel
{
    // ── Counts ────────────────────────────────────────
    public int TotalMembers { get; set; }
    public int ActiveMembers { get; set; }
    public int TotalTrainers { get; set; }
    public int ActiveTrainers { get; set; }
    public int TodayCheckIns { get; set; }
    public int CurrentlyInGym { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int UnpaidSubscriptions { get; set; }
    public decimal MonthlyRevenue { get; set; }

    // ── Lists ─────────────────────────────────────────
    public List<Member> RecentMembers { get; set; } = new();
    public List<Attendance> TodayAttendances { get; set; } = new();
    public List<Subscription> ExpiringThisWeek { get; set; } = new();
}