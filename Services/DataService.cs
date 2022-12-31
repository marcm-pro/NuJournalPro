#nullable disable
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NuJournalPro.Data;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using Microsoft.EntityFrameworkCore;
using NuJournalPro.Services.Interfaces;
using NuJournalPro.Areas.Identity.Pages.Account.Manage;

namespace NuJournalPro.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AddNewUserModel> _logger;
        private readonly IUserStore<NuJournalUser> _userStore;
        private readonly IUserEmailStore<NuJournalUser> _emailStore;
        private readonly OwnerSettings _ownerSettings;
        private readonly IEmailSender _emailSender;
        private readonly IImageService _imageService;
        private readonly DefaultUserSettings _defaultUserSettings;

        public DataService(ApplicationDbContext dbContext,
                          UserManager<NuJournalUser> userManager,
                          RoleManager<IdentityRole> roleManager,
                          ILogger<AddNewUserModel> logger,
                          IUserStore<NuJournalUser> userStore,
                          IOptions<OwnerSettings> ownerSettings,
                          IEmailSender emailSender,
                          IImageService imageService,
                          IOptions<DefaultUserSettings> defaultUserSettings)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _userStore = userStore;
            _emailStore = GetEmailStore();
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
            else
            {
                // If there are User Roles in the system, then check to see if any are missing.
                // If any are missing, then create them.
                foreach (var userRole in Enum.GetNames(typeof(NuJournalUserRole)))
                {
                    if (_dbContext.Roles.Any(r => r.Name == userRole) == false)
                    {
                        // Use the Role Manager to create roles.
                        await _roleManager.CreateAsync(new IdentityRole(userRole));
                    }
                }
            }
        }

        private async Task CreateAdminUserAsync()
        {
            // Check if there is a Owner user role.
            if (_dbContext.Roles.Any(r => r.Name == NuJournalUserRole.Owner.ToString())) {
                // Check if there is already an Owner account.
                
                var warnAfterCreation = _ownerSettings.WarnAfterCreation ?? Environment.GetEnvironmentVariable("WarnAfterCreation");

                foreach (var appUser in _userManager.Users.ToList())
                {
                    var appUserRole = String.Join(", ", await _userManager.GetRolesAsync(appUser)); // in case the user has more than one role
                    if (appUserRole.Contains(NuJournalUserRole.Owner.ToString())) {
                        if (appUser.Email.Equals(_ownerSettings.Email))
                        {
                            var ownerAlreadyExistsError = $"An Owner account for user {appUser} already exists and therefore a new account will not be created.";
                            _logger.LogError(ownerAlreadyExistsError);
                            Console.WriteLine(ownerAlreadyExistsError);
                            if (warnAfterCreation.ToUpper().Contains("YES") || warnAfterCreation.ToUpper().Contains("Y"))
                            {
                                await _emailSender.SendEmailAsync(appUser.Email,
                                                  $"The Owner Account ({appUser.DisplayName}) has already been created",
                                                  $"<p>The Owner Account <b>{appUser.DisplayName}</b> with the registered <b>{appUser.Email}</b> email address has already been created.</p><p>Please try logging in or reseting your password.</p><p>Thank you</p>");
                            }
                            return;
                        }
                        else
                        {
                            var ownerAlreadyExistsError = $"User {appUser} is the Owner of this application and an account for this user already exists.";
                            _logger.LogError(ownerAlreadyExistsError);
                            Console.WriteLine(ownerAlreadyExistsError);
                            if (warnAfterCreation.ToUpper().Contains("YES") || warnAfterCreation.ToUpper().Contains("Y"))
                            {
                                await _emailSender.SendEmailAsync(_ownerSettings.Email,
                                                  $"The Application already has an Owner Account ({appUser.DisplayName})",
                                                  $"<p>The Application already has an Owner Account (<b>{appUser.DisplayName}</b>) with the registered email address <b>{appUser.Email}</b>.</p><p>Please try logging in or reseting your password.</p><p>Thank you</p>");
                            }
                            return;
                        }
                    }
                }

                var ownerAccount = new NuJournalUser()
                {
                    Email = _ownerSettings.Email ?? Environment.GetEnvironmentVariable("Email"),
                    UserName = _ownerSettings.Email ?? Environment.GetEnvironmentVariable("Email"),
                    FirstName = _ownerSettings.FirstName ?? Environment.GetEnvironmentVariable("FirstName"),
                    MiddleName = _ownerSettings.MiddleName ?? Environment.GetEnvironmentVariable("MiddleName"),
                    LastName = _ownerSettings.LastName ?? Environment.GetEnvironmentVariable("LastName"),
                    DisplayName = _ownerSettings.DisplayName ?? Environment.GetEnvironmentVariable("DisplayName"),
                    ImageData = await _imageService.EncodeImageAsync(_defaultUserSettings.Avatar),
                    MimeType = _imageService.MimeType(_defaultUserSettings.Avatar),
                    CreatedByUser = "Data Service",
                    EmailConfirmed = true
                };

                var ownerPassword = GenerateRandomPassword();

                await _userStore.SetUserNameAsync(ownerAccount, ownerAccount.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(ownerAccount, ownerAccount.Email, CancellationToken.None);

                ownerAccount.UserRoles.Add(NuJournalUserRole.Owner.ToString());

                var ownerCreationResult = await _userManager.CreateAsync(ownerAccount, ownerPassword);

                if (ownerCreationResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(ownerAccount, NuJournalUserRole.Owner.ToString());
                    var ownerCreationMsgSuccess = $"User {ownerAccount.CreatedByUser} created a new account with the username {ownerAccount.CreatedByRolesString} and assigned this account the {NuJournalUserRole.Owner.ToString()} role.";
                    _logger.LogError(ownerCreationMsgSuccess);
                    Console.WriteLine(ownerCreationMsgSuccess);

                    await _emailSender.SendEmailAsync(_ownerSettings.Email,
                                  $"Temporary password for the Application Owner Account: {ownerAccount.DisplayName}",
                                  $"<p>Your temporary password for the Application Owner Account <b>{ownerAccount.DisplayName}</b> is: <b>{ownerPassword}</b></p><p>Please change your password after logging in.</p><p>Thank you</p>");

                }
                else
                {
                    var ownerFailedToCreate = $"The creation of Owner Account {ownerAccount.Email} failed due to the following reasons: " + String.Join(", ", ownerCreationResult.Errors);
                    _logger.LogError(ownerFailedToCreate);
                    Console.WriteLine(ownerFailedToCreate);
                }
            }
            else
            {
                var ownerCreationLogError = $"The {NuJournalUserRole.Owner.ToString()} user role is missing. Cannot proceed with creating the default Owner User.";
                _logger.LogError(ownerCreationLogError);
                Console.WriteLine(ownerCreationLogError);
            }
            
        }

        private string GenerateRandomPassword(PasswordOptions pwdOptions = null)
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

            if (pwdOptions.RequireUppercase) chars.Insert(rand.Next(0, chars.Count), characterPool[0][rand.Next(0, characterPool[0].Length)]);
            if (pwdOptions.RequireLowercase) chars.Insert(rand.Next(0, chars.Count), characterPool[1][rand.Next(0, characterPool[1].Length)]);
            if (pwdOptions.RequireDigit) chars.Insert(rand.Next(0, chars.Count), characterPool[2][rand.Next(0, characterPool[2].Length)]);
            if (pwdOptions.RequireNonAlphanumeric) chars.Insert(rand.Next(0, chars.Count), characterPool[3][rand.Next(0, characterPool[3].Length)]);

            for (int i = chars.Count; i < pwdOptions.RequiredLength || chars.Distinct().Count() < pwdOptions.RequiredUniqueChars; i++)
            {
                string rcs = characterPool[rand.Next(0, characterPool.Length)];
                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        private IUserEmailStore<NuJournalUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<NuJournalUser>)_userStore;
        }
    }
}
