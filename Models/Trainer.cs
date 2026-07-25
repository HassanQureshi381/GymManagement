namespace GymManagementSystem.Models;

public class Trainer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Member> Members { get; set; } = new List<Member>();
}