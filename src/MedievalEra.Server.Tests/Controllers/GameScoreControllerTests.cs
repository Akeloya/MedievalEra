using MedievalEra.Server.Controllers;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Scores;
using MedievalEra.Server.Core.Store;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using ProfileEntity = MedievalEra.Server.Core.Store.Entitites.PlayerEntity;

namespace MedievalEra.Server.Tests.Controllers
{
    [TestFixture]
    public class GameScoreControllerTests
    {
        [Test]
        public async Task Save_CalculatesAndPersistsAllPlayerScores()
        {
            await using var dbContext = new AppDbContext(
                AppDbContext.DatabaseType.InMemory,
                Guid.NewGuid().ToString());
            await dbContext.Database.EnsureCreatedAsync();

            var request = new GameScoreRequest
            {
                Players = Enumerable.Range(1, 4).Select(playerNumber => new PlayerScoreRequest
                {
                    PlayerNumber = playerNumber,
                    PlayerName = $"Игрок {playerNumber}",
                    Buildings =
                    [
                        new BuildingScoreRequest
                        {
                            Building = Buildings.ScorchedEarth,
                            Count = 1,
                            InsideWallCount = 1
                        },
                        new BuildingScoreRequest
                        {
                            Building = Buildings.Wall,
                            Count = 1,
                            InsideWallCount = 1
                        },
                        new BuildingScoreRequest
                        {
                            Building = Buildings.Tower,
                            Count = playerNumber == 1 ? 2 : 0,
                            InsideWallCount = playerNumber == 1 ? 1 : 0
                        }
                    ],
                    MarketBonus = playerNumber == 1 ? 3 : 0,
                    GuildResources = playerNumber == 1 ? 2 : 0,
                    UniversityCultureLevel = playerNumber == 1 ? 4 : 0,
                    CathedralDiceCount = playerNumber == 1 ? 1 : 0,
                    CultureIcons = playerNumber == 1 ? 2 : 0,
                    WalledAreaSize = playerNumber == 1 ? 2 : 0,
                    DisasterPoints = playerNumber == 1 ? 4 : 0
                }).ToList()
            };

            var controller = new GameScoreController(dbContext);
            var response = await controller.Save(request);

            Assert.That(response, Is.InstanceOf<CreatedAtActionResult>());

            var savedGame = await dbContext.GameScores
                .Include(game => game.Players)
                .ThenInclude(player => player.Buildings)
                .SingleAsync();
            var playerScore = savedGame.Players.Single(player => player.PlayerNumber == 1);

            Assert.That(playerScore.IntermediateTotal, Is.EqualTo(30));
            Assert.That(playerScore.FinalTotal, Is.EqualTo(26));
            Assert.That(playerScore.Buildings.Single(building => building.Building == Buildings.Tower).Points, Is.EqualTo(3));
            Assert.That(playerScore.Buildings
                .Where(building => building.Building is Buildings.ScorchedEarth or Buildings.Wall)
                .Sum(building => building.Points), Is.Zero);
            Assert.That(playerScore.Title, Is.EqualTo("Раб"));
            Assert.That(playerScore.TitleComment, Is.EqualTo("Терпимо. Продолжайте возделывать землю."));
            Assert.That(playerScore.MostDevelopedCultureBonus, Is.EqualTo(5));
            Assert.That(playerScore.LargestWalledAreaBonus, Is.EqualTo(10));
            Assert.That(playerScore.HasTitle, Is.True);
            var secondPlayer = savedGame.Players.Single(player => player.PlayerNumber == 2);
            Assert.That(secondPlayer.MostDevelopedCultureBonus, Is.Zero);
            Assert.That(secondPlayer.LargestWalledAreaBonus, Is.Zero);
            Assert.That(secondPlayer.HasTitle, Is.False);
        }

        [Test]
        public async Task Save_AwardsBothBonusesToEveryPlayerTiedForHighestMetric()
        {
            await using var dbContext = new AppDbContext(
                AppDbContext.DatabaseType.InMemory,
                Guid.NewGuid().ToString());
            await dbContext.Database.EnsureCreatedAsync();

            var request = new GameScoreRequest
            {
                Players = Enumerable.Range(1, 4).Select(playerNumber => new PlayerScoreRequest
                {
                    PlayerNumber = playerNumber,
                    PlayerName = $"Игрок {playerNumber}",
                    Buildings = [],
                    CultureIcons = 3,
                    WalledAreaSize = 4
                }).ToList()
            };

            var controller = new GameScoreController(dbContext);
            var response = await controller.Save(request);

            Assert.That(response, Is.InstanceOf<CreatedAtActionResult>());

            var savedPlayers = await dbContext.PlayerScores.ToListAsync();
            Assert.That(savedPlayers, Has.Count.EqualTo(4));
            Assert.That(savedPlayers.Select(player => player.MostDevelopedCultureBonus), Is.All.EqualTo(5));
            Assert.That(savedPlayers.Select(player => player.LargestWalledAreaBonus), Is.All.EqualTo(10));
            Assert.That(savedPlayers.Select(player => player.FinalTotal), Is.All.EqualTo(18));
        }

        [Test]
        public async Task Save_AssociatesRegisteredPlayerAndMarksTheirResultPending()
        {
            await using var dbContext = new AppDbContext(
                AppDbContext.DatabaseType.InMemory,
                Guid.NewGuid().ToString());
            await dbContext.Database.EnsureCreatedAsync();
            var registeredProfile = new ProfileEntity
            {
                Name = "Online player",
                CreatedAt = DateTime.UtcNow,
                PasswordHash = "password-hash"
            };
            dbContext.Players.Add(registeredProfile);
            await dbContext.SaveChangesAsync();

            var request = new GameScoreRequest
            {
                GameType = GameType.Online,
                Players = Enumerable.Range(1, 4).Select(playerNumber => new PlayerScoreRequest
                {
                    PlayerNumber = playerNumber,
                    PlayerName = playerNumber == 1 ? " online player " : $"Offline {playerNumber}",
                    Buildings = []
                }).ToList()
            };

            var response = await new GameScoreController(dbContext).Save(request);
            Assert.That(response, Is.InstanceOf<CreatedAtActionResult>());

            var savedGame = await dbContext.GameScores.Include(game => game.Players).SingleAsync();
            var savedRegisteredPlayer = savedGame.Players.Single(player => player.PlayerNumber == 1);
            Assert.That(savedGame.GameType, Is.EqualTo(GameType.Online));
            Assert.That(savedRegisteredPlayer.PlayerEntityId, Is.EqualTo(registeredProfile.Id));
            Assert.That(savedRegisteredPlayer.ResultAccepted, Is.False);
            Assert.That(savedGame.Players.Where(player => player.PlayerNumber != 1)
                .Select(player => player.ResultAccepted), Is.All.True);

            var historyResponse = await new GameScoreController(dbContext).List();
            Assert.That(historyResponse, Is.InstanceOf<OkObjectResult>());
            var historyJson = JsonSerializer.Serialize(((OkObjectResult)historyResponse).Value);
            using var history = JsonDocument.Parse(historyJson);
            Assert.That(history.RootElement[0].GetProperty("GameType").GetInt32(), Is.EqualTo((int)GameType.Online));
            Assert.That(history.RootElement[0].GetProperty("Players").GetArrayLength(), Is.EqualTo(4));
        }

        [Test]
        public async Task AcceptResult_OnlyAcceptsResultBelongingToSignedInProfile()
        {
            await using var dbContext = new AppDbContext(
                AppDbContext.DatabaseType.InMemory,
                Guid.NewGuid().ToString());
            await dbContext.Database.EnsureCreatedAsync();
            var profile = new ProfileEntity
            {
                Name = "Online player",
                CreatedAt = DateTime.UtcNow,
                PasswordHash = "password-hash"
            };
            dbContext.Players.Add(profile);
            await dbContext.SaveChangesAsync();
            var savedResult = new PlayerScore
            {
                PlayerNumber = 1,
                PlayerName = profile.Name,
                PlayerEntityId = profile.Id,
                ResultAccepted = false,
                GameScore = new GameScore()
            };
            dbContext.PlayerScores.Add(savedResult);
            await dbContext.SaveChangesAsync();

            var controller = new PlayerProfileController(dbContext, new PasswordHasher<ProfileEntity>());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, profile.Id.ToString())
                    ]))
                }
            };

            var profileResponse = await controller.GetCurrentProfile();
            Assert.That(profileResponse, Is.InstanceOf<OkObjectResult>());
            var profileJson = JsonSerializer.Serialize(((OkObjectResult)profileResponse).Value);
            using var profileDocument = JsonDocument.Parse(profileJson);
            Assert.That(profileDocument.RootElement.GetProperty("PendingResults").GetArrayLength(), Is.EqualTo(1));

            var response = await controller.AcceptResult(savedResult.Id);

            Assert.That(response, Is.InstanceOf<NoContentResult>());
            Assert.That((await dbContext.PlayerScores.SingleAsync()).ResultAccepted, Is.True);
        }
    }
}
