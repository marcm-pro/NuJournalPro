using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NuJournalPro.Data;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using Microsoft.EntityFrameworkCore;
using NuJournalPro.Services.Interfaces;

namespace NuJournalPro.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly OwnerSettings _ownerSettings;
        private readonly IEmailSender _emailSender;
        private readonly IImageService _imageService;
        private readonly DefaultUserSettings _defaultUserSettings;

        public DataService(ApplicationDbContext dbContext,
                          UserManager<NuJournalUser> userManager,
                          RoleManager<IdentityRole> roleManager,
                          IOptions<OwnerSettings> ownerSettings,
                          IEmailSender emailSender,
                          IImageService imageService,
                          IOptions<DefaultUserSettings> defaultUserSettings)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _ownerSettings = ownerSettings.Value;
            _emailSender = emailSender;
            _imageService = imageService;
            _defaultUserSettings = defaultUserSettings.Value;
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
            var ownerUser = new NuJournalUser()
            {
                Email = _ownerSettings.Email ?? Environment.GetEnvironmentVariable("Email")!,
                UserName = _ownerSettings.Email ?? Environment.GetEnvironmentVariable("Email")!,
                FirstName = _ownerSettings.FirstName ?? Environment.GetEnvironmentVariable("FirstName")!,
                MiddleName = _ownerSettings.MiddleName ?? Environment.GetEnvironmentVariable("MiddleName")!,
                LastName = _ownerSettings.LastName ?? Environment.GetEnvironmentVariable("LastName")!,
                DisplayName = _ownerSettings.DisplayName ?? Environment.GetEnvironmentVariable("DisplayName")!,
                ImageData = await _imageService.EncodeImageAsync(_defaultUserSettings.Avatar!),
                MimeType = _imageService.MimeType(_defaultUserSettings.Avatar!),
                EmailConfirmed = true
            };

            var warnAfterCreation = _ownerSettings.WarnAfterCreation ?? Environment.GetEnvironmentVariable("WarnAfterCreation")!;

            var ownerPassword = GenerateRandomPassword();

            var creationResult = await _userManager.CreateAsync(ownerUser, ownerPassword);

            if (creationResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(ownerUser, NuJournalUserRole.Owner.ToString());
                await _emailSender.SendEmailAsync(_ownerSettings.Email,
                                                  $"Temporary password for the Owner ({ownerUser.DisplayName}) account",
                                                  $"<p>Your temporary <b>{ownerUser.DisplayName}</b> account password is: <b>{ownerPassword}</b></p><p>Please change your password after logging in.</p>");
            }
            else if (warnAfterCreation.ToLower() == "y")
            {
                await _emailSender.SendEmailAsync(_ownerSettings.Email,
                                                  $"An Owner ({ownerUser.DisplayName}) account already exists",
                                                  $"<p>An account named <b>{ownerUser.DisplayName}</b> with the registered <b>{ownerUser.Email}</b> email address already exists.</p><p>Please try logging in or reseting your password.</p>");
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
