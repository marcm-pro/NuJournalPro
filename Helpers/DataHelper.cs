using Microsoft.EntityFrameworkCore;
using NuJournalPro.Data;

namespace NuJournalPro.Helpers
{
    public static class DataHelper
    {
        public static async Task ManageDataAsync(IServiceProvider serviceProvider)
        {
            // Get the DbContext instance.
            var dbContextService = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Migration: this is equivalent to update-database.
            await dbContextService.Database.MigrateAsync();
        }
    }
}
