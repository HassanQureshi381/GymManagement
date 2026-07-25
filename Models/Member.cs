using NuGet.DependencyResolver;

namespace GymManagementSystem.Models;

public class Member
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime JoinDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;

    // Navigation
    public int? TrainerId { get; set; }
    public Trainer? Trainer { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}