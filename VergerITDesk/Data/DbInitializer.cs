using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VergerITDesk.Models;
using VergerITDesk.Services;

namespace VergerITDesk.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (!db.KpiDefinitions.Any())
        {
            db.KpiDefinitions.AddRange(KpiCatalog.Definitions);
            await db.SaveChangesAsync();
        }
    }
}
