using MedievalEra.Server.Core.Store;
using MedievalEra.Server.Core.Store.Entitites;
using Microsoft.EntityFrameworkCore;

namespace MedievalEra.Server.Tests.Core.Store
{
    [TestFixture]
    public class DbContextTests
    {
        [Test]
        [TestCase(AppDbContext.DatabaseType.InMemory)]
        [TestCase(AppDbContext.DatabaseType.Sqlite)]
        public async Task InitDbContextMemoryTest(AppDbContext.DatabaseType type)
        {
            using var dbContext = new AppDbContext(type);
            var exists = await dbContext.Database.EnsureCreatedAsync();
            if (!exists)
            {
                exists = await dbContext.Database.EnsureDeletedAsync();
                Assert.That(exists, Is.True);
                exists = await dbContext.Database.EnsureCreatedAsync();
                Assert.That(exists, Is.True);
            }
            
            var player = new PlayerEntity() { Name = "test", CreatedAt = DateTime.Now };
            dbContext.Players.Attach(player);
            await dbContext.SaveChangesAsync();

            Assert.That(player.Id, Is.Not.Zero);

            var dbPlayer = dbContext.Players.Find(player.Id);

            Assert.That(dbPlayer, Is.Not.Null);
            Assert.That(dbPlayer.Name, Is.EqualTo(player.Name));
        }        

        [Test]
        public async Task EnsureCurrentSchema_UpgradesLegacySqliteWithoutDroppingProfiles()
        {
            await using var dbContext = new AppDbContext(
                AppDbContext.DatabaseType.Sqlite,
                "Data Source=:memory:");
            await dbContext.Database.OpenConnectionAsync();
            await dbContext.Database.ExecuteSqlRawAsync(
                "CREATE TABLE \"Players\" (\"Id\" INTEGER PRIMARY KEY, \"Name\" TEXT NOT NULL, \"CreatedAt\" TEXT NOT NULL, \"LastGame\" TEXT NULL)");
            await dbContext.Database.ExecuteSqlRawAsync(
                "CREATE TABLE \"GameScores\" (\"Id\" INTEGER PRIMARY KEY, \"CreatedAt\" TEXT NOT NULL)");
            await dbContext.Database.ExecuteSqlRawAsync(
                "CREATE TABLE \"PlayerScores\" (\"Id\" INTEGER PRIMARY KEY, \"GameScoreId\" INTEGER NOT NULL)");
            await dbContext.Database.ExecuteSqlRawAsync(
                "INSERT INTO \"Players\" (\"Name\", \"CreatedAt\") VALUES ('Player', '2026-01-01'), ('player', '2026-01-02')");

            await DatabaseSchemaInitializer.EnsureCurrentSchemaAsync(dbContext);

            var playerCount = await dbContext.Database.SqlQueryRaw<int>(
                "SELECT COUNT(*) AS \"Value\" FROM \"Players\"").SingleAsync();
            Assert.That(playerCount, Is.EqualTo(2));
        }
    }        
}
