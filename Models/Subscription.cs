namespace GymManagementSystem.Models;

public enum SubscriptionPlan
{
    Monthly,
    Quarterly,
    SemiAnnual,
    Annual
}

public class Subscription
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public Member? Member { get; set; }

    public SubscriptionPlan Plan { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsActive => DateTime.Now >= StartDate && DateTime.Now <= EndDate;
}