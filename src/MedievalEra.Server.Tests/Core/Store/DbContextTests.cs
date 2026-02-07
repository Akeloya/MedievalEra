using MedievalEra.Server.Core.Store;
using MedievalEra.Server.Core.Store.Entitites;

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
    }
}
