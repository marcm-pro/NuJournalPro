using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NuJournalPro.Data;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using Microsoft.EntityFrameworkCore;

namespace NuJournalPro.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AdminSettings _adminSettings;
        private readonly IEmailSender _emailSender;

        public DataService(ApplicationDbContext dbContext,
                          UserManager<NuJournalUser> userManager,
                          RoleManager<IdentityRole> roleManager,
                          IOptions<AdminSettings> adminSettings,
                          IEmailSender emailSender)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _adminSettings = adminSettings.Value;
            _emailSender = emailSender;
        }

        public async Task ManageDataAsync()
        {
            // Migration: this is equivalent to update-database.
            await _dbContext.Database.MigrateAsync();

            // Create the default Roles if they don't exist.
            await CreateUserRolesAsync();

            // Create the default Administrator User if it doesn't exist.
            await CreateAdminUserAsync();
        }

        private async Task CreateUserRolesAsync()
        {
            // If there are no User Roles in the system, then create them.
            if (_dbContext.Roles.Any() == false)
            {
                foreach (var userRole in Enum.GetNames(typeof(NuJournalUserRole)))
                {
                    // Use the Role Manager to create roles.
                    await _roleManager.CreateAsync(new IdentityRole(userRole));
                }
            }
        }

        private async Task CreateAdminUserAsync()
        {
            var adminUser = new NuJournalUser()
            {
                Email = _adminSettings.Email ?? Environment.GetEnvironmentVariable("Email")!,
                UserName = _adminSettings.Email ?? Environment.GetEnvironmentVariable("Email")!,
                FirstName = _adminSettings.FirstName ?? Environment.GetEnvironmentVariable("FirstName")!,
                MiddleName = _adminSettings.MiddleName ?? Environment.GetEnvironmentVariable("MiddleName")!,
                LastName = _adminSettings.LastName ?? Environment.GetEnvironmentVariable("LastName")!,
                DisplayName = _adminSettings.DisplayName ?? Environment.GetEnvironmentVariable("DisplayName")!,
                EmailConfirmed = true
            };

            var WarnAfterCreation = _adminSettings.WarnAfterCreation ?? Environment.GetEnvironmentVariable("WarnAfterCreation")!;

            var adminPassword = GenerateRandomPassword();

            var creationResult = await _userManager.CreateAsync(adminUser, adminPassword);

            if (creationResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, NuJournalUserRole.Administrator.ToString());
                await _emailSender.SendEmailAsync(_adminSettings.Email,
                                                  $"Temporary password for the {adminUser.DisplayName} account.",
                                                  $"<p>Your temporary <b>{adminUser.DisplayName}</b> account password is: <b>{adminPassword}</b></p><p>Please change your password after logging in.</p>");
            }
            else if (WarnAfterCreation.ToLower() == "y")
            {
                await _emailSender.SendEmailAsync(_adminSettings.Email,
                                                  $"{adminUser.DisplayName} account already exists.",
                                                  $"<p>An account named <b>{adminUser.DisplayName}</b> with the registered <b>{adminUser.Email}</b> email address already exists.</p><p>Please try logging in or reseting your password.</p>");
            }
        }

        private string GenerateRandomPassword(PasswordOptions? pwdOptions = null)
        {
            if (pwdOptions == null)
            {
                pwdOptions = new PasswordOptions()
                {
                    RequiredLength = 16,
                    RequiredUniqueChars = 4,
                    RequireDigit = true,
                    RequireLowercase = true,
                    RequireNonAlphanumeric = true,
                    RequireUppercase = true
                };
            }

            string[] characterPool = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",
                "abcdefghijkmnopqrstuvwxyz",
                "0123456789",
                "!@$?_-"
            };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (pwdOptions.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    characterPool[0][rand.Next(0, characterPool[0].Length)]);

            if (pwdOptions.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    characterPool[1][rand.Next(0, characterPool[1].Length)]);

            if (pwdOptions.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    characterPool[2][rand.Next(0, characterPool[2].Length)]);

            if (pwdOptions.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    characterPool[3][rand.Next(0, characterPool[3].Length)]);

            for (int i = chars.Count; i < pwdOptions.RequiredLength || chars.Distinct().Count() < pwdOptions.RequiredUniqueChars; i++)
            {
                string rcs = characterPool[rand.Next(0, characterPool.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
    }
}
