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
        [TestCase(Buildings.ScorchedEarth, false, false)]
        [TestCase(Buildings.ScorchedEarth, true, false)]
        [TestCase(Buildings.ScorchedEarth, false, true)]
        [TestCase(Buildings.Wall, false, false)]
        [TestCase(Buildings.Wall, true, false)]
        [TestCase(Buildings.Wall, false, true)]
        public async Task Save_NonScoringBuildingsRespectSetAndWallLimits(
            Buildings type, bool exceedSet, bool exceedInsideWall)
        {
            await using var dbContext = new AppDbContext(
                AppDbContext.DatabaseType.InMemory, Guid.NewGuid().ToString());
            var definition = new MedievalEra.Server.Core.Game.BuildingsFactory().Get(type);
            Assert.That(definition.Score, Is.Null);
            var request = new GameScoreRequest
            {
                Players = Enumerable.Range(1, 4).Select(number => new PlayerScoreRequest
                {
                    PlayerNumber = number,
                    PlayerName = $"Игрок {number}",
                    Buildings = number == 1
                        ? [new BuildingScoreRequest
                        {
                            Building = type, Count = definition.Quantity,
                            InsideWallCount = definition.Quantity + (exceedInsideWall ? 1 : 0)
                        }, new BuildingScoreRequest { Building = Buildings.Tower, Count = 1, InsideWallCount = 1 }]
                        : number == 2 && exceedSet
                            ? [new BuildingScoreRequest { Building = type, Count = 1 }]
                            : []
                }).ToList()
            };

            var response = await new GameScoreController(dbContext).Save(request);

            if (exceedSet || exceedInsideWall)
            {
                Assert.That(response, Is.InstanceOf<BadRequestObjectResult>());
                Assert.That(await dbContext.GameScores.CountAsync(), Is.Zero);
                return;
            }

            Assert.That(response, Is.InstanceOf<CreatedAtActionResult>());
            var player = await dbContext.PlayerScores.Include(player => player.Buildings)
                .SingleAsync(player => player.PlayerNumber == 1);
            var building = player.Buildings.Single(building => building.Building == type);
            Assert.That(building.Count, Is.EqualTo(definition.Quantity));
            Assert.That(building.InsideWallCount, Is.EqualTo(definition.Quantity));
            Assert.That(building.Points, Is.Zero);
            Assert.That(player.IntermediateTotal, Is.EqualTo(2));
            Assert.That(player.FinalTotal, Is.EqualTo(2));
        }

        [TestCase(2, 0, 0, false)]
        [TestCase(1, 1, 0, false)]
        [TestCase(1, 0, 2, false)]
        [TestCase(0, 0, 1, false)]
        [TestCase(-1, 0, 0, false)]
        [TestCase(1, 0, -1, false)]
        [TestCase(1, 0, 0, true)]
        public async Task Save_RejectsInvalidBuildingCountsWithoutPersisting(
            int firstCount, int secondCount, int insideWall, bool duplicate)
        {
            await using var dbContext = new AppDbContext(
                AppDbContext.DatabaseType.InMemory, Guid.NewGuid().ToString());
            var request = new GameScoreRequest
            {
                Players = Enumerable.Range(1, 4).Select(number => new PlayerScoreRequest
                {
                    PlayerNumber = number,
                    PlayerName = $"Игрок {number}",
                    Buildings = [new BuildingScoreRequest
                    {
                        Building = Buildings.Tower,
                        Count = number == 1 ? firstCount : number == 2 ? secondCount : 0,
                        InsideWallCount = number == 1 ? insideWall : 0
                    }]
                }).ToList()
            };
            if (duplicate) request.Players[0].Buildings!.Add(new BuildingScoreRequest { Building = Buildings.Tower });

            var response = await new GameScoreController(dbContext).Save(request);

            Assert.That(response, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(await dbContext.GameScores.CountAsync(), Is.Zero);
            Assert.That(await dbContext.Players.CountAsync(), Is.Zero);
            Assert.That(dbContext.ChangeTracker.HasChanges(), Is.False);
        }

        [TestCase(Buildings.Empty)]
        [TestCase((Buildings)999)]
        public async Task Save_RejectsTypesOutsideGameSet(Buildings type)
        {
            await using var dbContext = new AppDbContext(
                AppDbContext.DatabaseType.InMemory, Guid.NewGuid().ToString());
            var request = new GameScoreRequest
            {
                Players = Enumerable.Range(1, 4).Select(number => new PlayerScoreRequest
                {
                    PlayerNumber = number, PlayerName = $"Игрок {number}",
                    Buildings = [new BuildingScoreRequest { Building = type }]
                }).ToList()
            };
            Assert.That(await new GameScoreController(dbContext).Save(request), Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public void Buildings_ReturnsFactoryGameSet()
        {
            var response = (OkObjectResult)new BuildingsController().Get();
            using var json = JsonDocument.Parse(JsonSerializer.Serialize(response.Value,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)));
            var factory = new MedievalEra.Server.Core.Game.BuildingsFactory();
            Assert.That(json.RootElement.GetArrayLength(), Is.EqualTo(MedievalEra.Server.Core.Game.BuildingsFactory.GameSet.Count));
            foreach (var item in json.RootElement.EnumerateArray())
            {
                var type = (Buildings)item.GetProperty("id").GetInt32();
                var building = factory.Get(type);
                Assert.That(factory.CanBuild(type), Is.True);
                Assert.That(item.GetProperty("label").GetString(), Is.EqualTo(building.Name));
                var points = item.GetProperty("points");
                Assert.That(points.ValueKind == JsonValueKind.Null ? (int?)null : points.GetInt32(), Is.EqualTo(building.Score));
                Assert.That(item.GetProperty("quantity").GetInt32(), Is.EqualTo(building.Quantity));
                Assert.That(item.GetProperty("key").GetString(), Is.EqualTo(type.ToString()));
            }
            Assert.That(factory.CanBuild(Buildings.Empty), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => factory.Get(Buildings.Empty));
        }

        [Test]
        public void Titles_ReturnsRangesUsedForScoreCalculation()
        {
            using var dbContext = new AppDbContext(
                AppDbContext.DatabaseType.InMemory,
                Guid.NewGuid().ToString());
            var controller = new GameScoreController(dbContext);

            var response = controller.Titles() as OkObjectResult;
            var json = JsonSerializer.Serialize(response?.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var ranges = JsonSerializer.Deserialize<List<GameScoreTitleRange>>(json,
                new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

            Assert.That(ranges, Has.Count.EqualTo(7));
            foreach (var score in new[] { int.MinValue, 44, 45, 64, 65, 79, 80, 89, 90, 94, 95, 99, 100, int.MaxValue })
            {
                var range = ranges.First(range => range.MaxScore is null || score <= range.MaxScore.Value);
                Assert.That((range.Title, range.Comment), Is.EqualTo(GameScoreTitle.ForScore(score)));
            }
        }

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
                            Building = Buildings.Wall,
                            Count = playerNumber == 1 ? 1 : 0,
                            InsideWallCount = 0
                        },
                        new BuildingScoreRequest
                        {
                            Building = Buildings.Tower,
                            Count = playerNumber == 1 ? 1 : 0,
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

            Assert.That(playerScore.IntermediateTotal, Is.EqualTo(29));
            Assert.That(playerScore.FinalTotal, Is.EqualTo(25));
            Assert.That(playerScore.Buildings.Single(building => building.Building == Buildings.Tower).Points, Is.EqualTo(2));
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
