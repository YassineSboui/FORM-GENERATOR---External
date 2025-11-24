using Microsoft.AspNetCore.Identity;
using NeoForm_Externe.Models;
using Microsoft.Extensions.Logging;

namespace NeoForm_Externe.Services.Configuration
{
    public class DatabaseSeeder
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();

            logger.LogInformation("🔧 DatabaseSeeder: Starting database initialization...");

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            try
            {
                // Create roles
                string[] roleNames = { "SuperAdmin", "Admin" };
                logger.LogInformation("🔧 DatabaseSeeder: Creating roles if they don't exist...");

                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        logger.LogInformation("🔧 DatabaseSeeder: Creating role '{RoleName}'", roleName);
                        var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));

                        if (roleResult.Succeeded)
                        {
                            logger.LogInformation("✅ DatabaseSeeder: Role '{RoleName}' created successfully", roleName);
                        }
                        else
                        {
                            logger.LogError("❌ DatabaseSeeder: Failed to create role '{RoleName}': {Errors}",
                                roleName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        logger.LogInformation("ℹ️  DatabaseSeeder: Role '{RoleName}' already exists", roleName);
                    }
                }

                // Create SuperAdmin user
                var superAdminUsername = "superadmin";
                var superAdminEmail = "superadmin@neoform.com";
                var superAdminPassword = "SuperAdmin@123"; // Change this in production!

                logger.LogInformation("🔧 DatabaseSeeder: Checking for SuperAdmin user '{Username}'", superAdminUsername);
                var superAdmin = await userManager.FindByNameAsync(superAdminUsername);

                if (superAdmin == null)
                {
                    logger.LogInformation("🔧 DatabaseSeeder: Creating SuperAdmin user '{Username}'", superAdminUsername);

                    superAdmin = new ApplicationUser
                    {
                        UserName = superAdminUsername,
                        Email = superAdminEmail,
                        FullName = "Super Administrator",
                        EmailConfirmed = true,
                        MustChangePassword = true // Force password change on first login
                    };

                    var result = await userManager.CreateAsync(superAdmin, superAdminPassword);
                    if (result.Succeeded)
                    {
                        logger.LogInformation("✅ DatabaseSeeder: SuperAdmin user created successfully: {Username}", superAdminUsername);

                        var roleResult = await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                        if (roleResult.Succeeded)
                        {
                            logger.LogInformation("✅ DatabaseSeeder: SuperAdmin role assigned to user '{Username}'", superAdminUsername);
                            Console.WriteLine($"✅ SuperAdmin user created: {superAdminUsername} / {superAdminPassword}");
                        }
                        else
                        {
                            logger.LogError("❌ DatabaseSeeder: Failed to assign SuperAdmin role: {Errors}",
                                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                        logger.LogError("❌ DatabaseSeeder: Failed to create SuperAdmin user: {ErrorMessage}", errorMessage);
                        Console.WriteLine($"❌ Failed to create SuperAdmin: {errorMessage}");
                    }
                }
                else
                {
                    logger.LogInformation("ℹ️  DatabaseSeeder: SuperAdmin user already exists: {Username}", superAdminUsername);
                    Console.WriteLine($"ℹ️  SuperAdmin already exists: {superAdminUsername}");
                }

                logger.LogInformation("✅ DatabaseSeeder: Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ DatabaseSeeder: Fatal error during database initialization");
                throw;
            }
        }
    }
}
