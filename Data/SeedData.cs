using GymManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace GymManagementSystem.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // ── Step 1: Create Roles ──────────────────────
        string[] roles = { "Admin", "Operator" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine(result.Succeeded
                    ? $"✅ Role '{role}' created."
                    : $"❌ Role '{role}' failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            else
            {
                Console.WriteLine($"ℹ️  Role '{role}' already exists.");
            }
        }

        // ── Step 2: Create Admin User ─────────────────
        await CreateUserAsync(
            userManager,
            email: "admin@gym.com",
            password: "Admin@123",
            fullName: "System Admin",
            role: "Admin"
        );

        // ── Step 3: Create Operator User ──────────────
        await CreateUserAsync(
            userManager,
            email: "operator@gym.com",
            password: "Operator@123",
            fullName: "Gym Operator",
            role: "Operator"
        );
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string role)
    {
        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = email,   // ← MUST match email exactly
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                NormalizedEmail = email.ToUpper(),
                NormalizedUserName = email.ToUpper()
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
                Console.WriteLine($"✅ User '{email}' created with role '{role}'.");
            }
            else
            {
                Console.WriteLine($"❌ User '{email}' failed:");
                foreach (var error in result.Errors)
                    Console.WriteLine($"   - {error.Description}");
            }
        }
        else
        {
            // Make sure role is assigned even if user already exists
            if (!await userManager.IsInRoleAsync(existingUser, role))
            {
                await userManager.AddToRoleAsync(existingUser, role);
                Console.WriteLine($"ℹ️  Role '{role}' assigned to existing user '{email}'.");
            }
            else
            {
                Console.WriteLine($"ℹ️  User '{email}' already exists with role '{role}'.");
            }
        }
    }
}