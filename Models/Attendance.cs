namespace GymManagementSystem.Models;

public class Attendance
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public Member? Member { get; set; }

    public DateTime CheckIn { get; set; } = DateTime.Now;
    public DateTime? CheckOut { get; set; }
    public string? Notes { get; set; }

    public TimeSpan? Duration => CheckOut.HasValue
        ? CheckOut.Value - CheckIn
        : null;
}