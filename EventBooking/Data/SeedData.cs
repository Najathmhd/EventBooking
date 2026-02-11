using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using EventBooking.Models;
using EventBooking.Data;

namespace EventBooking.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roleNames = { "Admin", "Organizer", "Member" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create Admin User
            var adminEmail = "najamhd037@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, "Admin@123");
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Create or Sync Admin Member Profile
            var adminMember = context.Members.FirstOrDefault(m => m.Email == adminEmail);
            if (adminMember == null)
            {
                context.Members.Add(new Member
                {
                    FullName = "System Admin",
                    Email = adminEmail,
                    PhoneNumber = "0000000000",
                    UserId = adminUser.Id
                });
            }
            else
            {
                adminMember.UserId = adminUser.Id;
            }
            await context.SaveChangesAsync();

            // Cleanup old hardcoded admin if it exists
            var oldAdminEmail = "admin@eventbooking.com";
            var oldAdminUser = await userManager.FindByEmailAsync(oldAdminEmail);
            if (oldAdminUser != null)
            {
                var oldMember = context.Members.FirstOrDefault(m => m.Email == oldAdminEmail);
                if (oldMember != null) context.Members.Remove(oldMember);
                await userManager.DeleteAsync(oldAdminUser);
                await context.SaveChangesAsync();
            }

            // 🔹 Auto-Price Update for Existing Metropolitan Events
            // Ensures the discovery hub looks premium by assigning value to legacy events.
            var premiumEvents = context.Events.Where(e => e.Price == 0).ToList();
            if (premiumEvents.Any())
            {
                var random = new Random();
                foreach (var evt in premiumEvents)
                {
                    // Assign a "premium" price between £35 and £150
                    evt.Price = random.Next(7, 31) * 5; 
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
