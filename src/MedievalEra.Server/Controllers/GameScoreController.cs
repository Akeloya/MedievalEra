using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game;
using MedievalEra.Server.Core.Game.Scores;
using MedievalEra.Server.Core.Store;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedievalEra.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameScoreController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public GameScoreController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("titles")]
        public IActionResult Titles() => Ok(GameScoreTitle.Ranges);

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] GameScoreRequest request)
        {
            if (!Enum.IsDefined(request.GameType))
            {
                return BadRequest("Неизвестный тип игры.");
            }

            if (request.Players is null || request.Players.Count is < 2 or > 4 ||
                request.Players.Select(player => player.PlayerNumber).Order()
                    .SequenceEqual(Enumerable.Range(1, request.Players.Count)) is false)
            {
                return BadRequest("Необходимо передать результаты от двух до четырех игроков с номерами по порядку.");
            }

            var score = new GameScore { GameType = request.GameType };
            var factory = new BuildingsFactory();
            // Проверяем общий запас до расчётов и изменений профилей игроков.
            foreach (var input in request.Players)
            {
                if (input.Buildings is null || input.Buildings.Any(building =>
                    !factory.CanBuild(building.Building) || building.Count < 0 ||
                    building.InsideWallCount < 0 || building.InsideWallCount > building.Count) ||
                    input.Buildings.Select(building => building.Building).Distinct().Count() != input.Buildings.Count)
                {
                    return BadRequest($"Некорректные здания игрока {input.PlayerNumber}.");
                }
            }

            foreach (var building in BuildingsFactory.GameSet)
            {
                var built = request.Players.SelectMany(player => player.Buildings!)
                    .Where(item => item.Building == building.Type).Sum(item => (long)item.Count);
                if (built > building.Quantity)
                {
                    return BadRequest($"Превышен запас здания «{building.Name}»: построено {built}, в наборе {building.Quantity}.");
                }
            }

            var profiles = await _dbContext.Players.ToListAsync();

            foreach (var input in request.Players)
            {
                if (string.IsNullOrWhiteSpace(input.PlayerName) || input.PlayerName.Length > 50 ||
                    input.Buildings is null ||
                    input.Buildings.Any(building =>
                        !Enum.IsDefined(building.Building) ||
                        building.Count < 0 ||
                        building.InsideWallCount < 0 ||
                        building.InsideWallCount > building.Count) ||
                    input.Buildings.Select(building => building.Building).Distinct().Count() != input.Buildings.Count ||
                    input.MarketBonus < 0 ||
                    input.MarketBonus > input.Buildings
                        .Where(building => building.Building == Buildings.Market)
                        .Sum(building => building.Count) * 8 ||
                    input.GuildResources < 0 ||
                    input.UniversityCultureLevel < 0 ||
                    input.CathedralDiceCount < 0 ||
                    input.CultureIcons < 0 ||
                    input.WalledAreaSize < 0 ||
                    input.DisasterPoints < 0)
                {
                    return BadRequest($"Некорректные данные игрока {input.PlayerNumber}.");
                }

                var buildings = input.Buildings.Select(building =>
                {
                    var insideWallCount = building.Building == Buildings.Tower
                        ? building.Count
                        : input.WallCompleted == false ? 0 : building.InsideWallCount;
                    return new BuildingScore
                    {
                        Building = building.Building,
                        Count = building.Count,
                        InsideWallCount = insideWallCount,
                        Points = (factory.Get(building.Building).Score ?? 0) *
                                 (building.Count + insideWallCount)
                    };
                }).ToList();

                int BuiltCount(Buildings type) => input.Buildings
                    .Where(building => building.Building == type)
                    .Sum(building => building.Count);

                var hasMarket = BuiltCount(Buildings.Market) > 0;
                var hasGuild = BuiltCount(Buildings.Guild) > 0;
                var hasUniversity = BuiltCount(Buildings.University) > 0;
                var hasCathedral = BuiltCount(Buildings.Cathedral) > 0;
                var cathedralDiceCount = hasCathedral
                    ? input.Buildings
                        .Where(building => factory.Get(building.Building).DiceType != DiceType.None)
                        .Sum(building => building.Count)
                    : 0;

                var buildingTotal = buildings.Sum(building => building.Points);
                var intermediateTotal = buildingTotal +
                                        (hasMarket ? input.MarketBonus : 0) +
                                        (hasGuild ? input.GuildResources : 0) +
                                        (hasUniversity ? input.UniversityCultureLevel : 0) +
                                        cathedralDiceCount +
                                        input.CultureIcons;

                var playerName = input.PlayerName.Trim();
                var profile = profiles.FirstOrDefault(candidate =>
                    string.Equals(candidate.Name, playerName, StringComparison.OrdinalIgnoreCase));
                if (profile is null)
                {
                    profile = new Core.Store.Entitites.PlayerEntity
                    {
                        Name = playerName,
                        CreatedAt = DateTime.UtcNow
                    };
                    profiles.Add(profile);
                    _dbContext.Players.Add(profile);
                }
                profile.LastGame = DateTime.UtcNow;

                score.Players.Add(new PlayerScore
                {
                    PlayerNumber = input.PlayerNumber,
                    PlayerName = playerName,
                    PlayerEntity = profile,
                    ResultAccepted = profile.PasswordHash is null,
                    Buildings = buildings,
                    MarketBonus = hasMarket ? input.MarketBonus : 0,
                    GuildResources = hasGuild ? input.GuildResources : 0,
                    UniversityCultureLevel = hasUniversity ? input.UniversityCultureLevel : 0,
                    CathedralDiceCount = cathedralDiceCount,
                    CultureIcons = input.CultureIcons,
                    WalledAreaSize = input.WallCompleted == false ? 0 : input.WalledAreaSize,
                    DisasterPoints = input.DisasterPoints,
                    IntermediateTotal = intermediateTotal
                });
            }

            var highestCultureIcons = score.Players.Max(player => player.CultureIcons);
            var largestWalledArea = score.Players.Max(player => player.WalledAreaSize);
            foreach (var player in score.Players)
            {
                if (highestCultureIcons > 0 && player.CultureIcons == highestCultureIcons)
                {
                    player.MostDevelopedCultureBonus = 5;
                }

                if (largestWalledArea > 0 && player.WalledAreaSize == largestWalledArea)
                {
                    player.LargestWalledAreaBonus = 10;
                }

                player.IntermediateTotal += player.MostDevelopedCultureBonus + player.LargestWalledAreaBonus;
                player.FinalTotal = player.IntermediateTotal - player.DisasterPoints;
                (player.Title, player.TitleComment) = GameScoreTitle.ForScore(player.FinalTotal);
            }

            var highestScore = score.Players.Max(player => player.FinalTotal);
            foreach (var player in score.Players)
            {
                player.HasTitle = player.FinalTotal == highestScore;
            }

            _dbContext.GameScores.Add(score);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = score.Id }, new { score.Id });
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var scores = await _dbContext.GameScores
                .AsNoTracking()
                .OrderByDescending(game => game.CreatedAt)
                .Select(game => new
                {
                    game.Id,
                    game.CreatedAt,
                    game.GameType,
                    Players = game.Players.OrderBy(player => player.PlayerNumber).Select(player => new
                    {
                        PlayerName = player.PlayerEntity.Name,
                        player.PlayerEntityId,
                        player.Title,
                        player.FinalTotal,
                        player.HasTitle,
                        player.ResultAccepted
                    })
                })
                .ToListAsync();

            var playerEntityIds = scores.SelectMany(game => game.Players)
                .Select(player => player.PlayerEntityId)
                .Distinct()
                .ToArray();
            var playerHistory = await _dbContext.PlayerScores
                .AsNoTracking()
                .Where(result => playerEntityIds.Contains(result.PlayerEntityId))
                .Select(result => new
                {
                    result.PlayerEntityId,
                    result.GameScoreId,
                    result.Title,
                    result.HasTitle
                })
                .ToListAsync();
            var playerStatistics = playerHistory
                .GroupBy(result => result.PlayerEntityId)
                .ToDictionary(group => group.Key, group =>
                {
                    var titles = group
                        .GroupBy(result => result.Title)
                        .Select(titleGroup => new
                        {
                            Title = titleGroup.Key,
                            Count = titleGroup.Select(result => result.GameScoreId).Distinct().Count()
                        })
                        .OrderByDescending(title => title.Count)
                        .ThenBy(title => title.Title)
                        .ToList();

                    return new
                    {
                        GamesPlayed = group.Select(result => result.GameScoreId).Distinct().Count(),
                        Wins = group.Where(result => result.HasTitle)
                            .Select(result => result.GameScoreId)
                            .Distinct()
                            .Count(),
                        MostCommonTitle = titles.FirstOrDefault()?.Title,
                        Titles = titles
                    };
                });

            return Ok(scores.Select(game => new
            {
                game.Id,
                game.CreatedAt,
                game.GameType,
                Players = game.Players.Select(player => new
                {
                    player.PlayerName,
                    player.Title,
                    player.FinalTotal,
                    player.HasTitle,
                    player.ResultAccepted,
                    Statistics = playerStatistics[player.PlayerEntityId]
                })
            }));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var score = await _dbContext.GameScores
                .Include(game => game.Players)
                .ThenInclude(player => player.Buildings)
                .SingleOrDefaultAsync(game => game.Id == id);

            return score is null ? NotFound() : Ok(score);
        }

    }

    public class GameScoreRequest
    {
        public GameType GameType { get; set; } = GameType.Tabletop;
        public List<PlayerScoreRequest>? Players { get; set; } = [];
    }

    public class PlayerScoreRequest
    {
        public int PlayerNumber { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public List<BuildingScoreRequest>? Buildings { get; set; } = [];
        public int MarketBonus { get; set; }
        public int GuildResources { get; set; }
        public int UniversityCultureLevel { get; set; }
        public int CathedralDiceCount { get; set; }
        public int CultureIcons { get; set; }
        public int WalledAreaSize { get; set; }
        public int DisasterPoints { get; set; }
        public bool? WallCompleted { get; set; }
    }

    public class BuildingScoreRequest
    {
        public Buildings Building { get; set; }
        public int Count { get; set; }
        public int InsideWallCount { get; set; }
    }
}
