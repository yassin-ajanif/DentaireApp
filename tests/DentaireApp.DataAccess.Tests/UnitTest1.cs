using DentaireApp.DataAccess.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DentaireApp.DataAccess.Tests;

public class UnitTest1
{
    [Fact]
    public async Task DbContext_CanInitializeSqliteInMemory()
    {
        var connectionString = $"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connectionString)
            .Options;

        await using var db = new AppDbContext(options);
        var created = await db.Database.EnsureCreatedAsync();

        Assert.True(created || await db.Database.CanConnectAsync());
    }
}
