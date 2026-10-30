using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedievalEra.Server.Core.Game;
using MedievalEra.Server.Core.Game.Dice;
using MedievalEra.Server.Core.Game.Player;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Scores;
using MedievalEra.Server.Core.Store;

namespace MedievalEra.Server.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public sealed class LobbyController(DiceFactory diceFactory, AppDbContext dbContext) : ControllerBase
{
    private const int MaxPlayers = 4;
    private static readonly ConcurrentDictionary<int, LobbyGame> Games = new();
    private static readonly ConcurrentDictionary<int, LobbyPresence> Presence = new();
    private static readonly TimeSpan WaitingLifetime = TimeSpan.FromHours(2);
    private static int _nextGameId;

    [HttpGet]
    public IActionResult GetLobby()
    {
        CleanupExpiredGames();
        if (TryGetPlayer(out var player))
            Presence[player.Id] = new LobbyPresence(player, DateTimeOffset.UtcNow);
        CleanupExpiredPresence();
        var playingPlayerIds = Games.Values.Where(game => game.Status == "playing")
            .SelectMany(game => game.Players.Where(member => member.Id.HasValue && !member.HasLeft).Select(member => member.Id!.Value))
            .ToHashSet();
        var waitingPlayerIds = Games.Values.Where(game => game.Status == "waiting")
            .SelectMany(game => game.Players.Where(member => member.Id.HasValue).Select(member => member.Id!.Value))
            .ToHashSet();
        var onlinePlayerIds = Presence.Keys.ToHashSet();
        return Ok(new
        {
            waitingPlayers = onlinePlayerIds.Union(waitingPlayerIds).Except(playingPlayerIds).Count(),
            playingPlayers = playingPlayerIds.Count,
            games = Games.Values.OrderBy(game => game.Id).Select(ToResponse)
        });
    }

    [HttpPost("games")]
    public IActionResult CreateGame([FromBody] CreateLobbyGameRequest request)
    {
        if (!TryGetPlayer(out var player)) return Unauthorized();
        if (request.Version is not ("base" or "rivers-and-roads"))
            return BadRequest("Выберите версию игры.");
        if (request.Mode is not ("sir-philipp" or "players") ||
            (request.Mode == "players" && request.OpponentsCount is < 1 or > 3))
            return BadRequest("Выберите допустимый режим и количество игроков.");

        var existing = Games.Values.FirstOrDefault(game => game.Players.Any(member => member.Id == player.Id && !member.HasLeft));
        if (existing is not null) return Conflict(new { message = "Вы уже зарегистрированы в игре.", gameId = existing.Id });

        var game = new LobbyGame(Interlocked.Increment(ref _nextGameId), request.Version, request.Mode, request.AllowBots, diceFactory);
        game.OwnerId = player.Id;
        game.Players.Add(new LobbyMember(player.Id, player.Name));
        game.Session.AddPlayer(new Player(player.Id, player.Name));
        if (request.Mode == "sir-philipp")
        {
            game.Players.Add(new LobbyMember(null, "Сэр Филипп", false, true));
            game.Session.AddPlayer(new Player(-1, "Сэр Филипп"));
            game.Session.Start();
            game.Status = "playing";
            game.StartedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            for (var index = 0; index < request.OpponentsCount; index++)
                game.Players.Add(new LobbyMember(null, $"Ожидает игрока {index + 2}", true));
        }
        Games[game.Id] = game;
        return Ok(ToResponse(game));
    }

    [HttpPost("games/{gameId:int}/join")]
    public IActionResult JoinGame(int gameId)
    {
        if (!TryGetPlayer(out var player)) return Unauthorized();
        CleanupExpiredGames();
        var game = Games.GetValueOrDefault(gameId);
        if (game is null) return NotFound();
        lock (game)
        {
            if (game.Status != "waiting") return Conflict(new { message = "Игра уже началась." });
            if (game.Players.Any(member => member.Id == player.Id)) return Ok(ToResponse(game));
            if (Games.Values.Any(candidate => candidate.Players.Any(member => member.Id == player.Id && !member.HasLeft)))
                return Conflict(new { message = "Вы уже зарегистрированы в другой игре." });
            var openSeat = game.Players.FirstOrDefault(member => member.WaitingForPlayer);
            if (openSeat is null) return Conflict(new { message = "В этой игре нет свободного места для игрока." });
            openSeat.Id = player.Id;
            openSeat.Name = player.Name;
            openSeat.WaitingForPlayer = false;
            game.Session.AddPlayer(new Player(player.Id, player.Name));
            return Ok(ToResponse(game));
        }
    }

    [HttpPost("games/{gameId:int}/start")]
    public IActionResult StartGame(int gameId, [FromQuery] bool fillWithBots = false)
    {
        if (!TryGetPlayer(out var player)) return Unauthorized();
        if (!Games.TryGetValue(gameId, out var game)) return NotFound();
        lock (game)
        {
            if (game.OwnerId != player.Id) return Forbid();
            if (game.Status != "waiting") return Conflict(new { message = "Игра уже началась." });
            var emptySeats = game.Players.Where(member => member.WaitingForPlayer).ToList();
            if (emptySeats.Count > 0 && (!fillWithBots || !game.AllowBots))
                return Conflict(new { message = "Не все места заняты игроками." });
            if (emptySeats.Count > 0)
            {
                for (var index = 0; index < emptySeats.Count; index++)
                {
                    emptySeats[index].Name = $"Игрок компьютер {index + 1}";
                    emptySeats[index].WaitingForPlayer = false;
                    emptySeats[index].IsComputer = true;
                }
            }
            foreach (var bot in game.Players.Where(member => member.IsComputer))
                game.Session.AddPlayer(new Player(-1 - game.Players.IndexOf(bot), bot.Name));
            game.Session.Start();
            game.Status = "playing";
            game.StartedAt = DateTimeOffset.UtcNow;
            return Ok(ToResponse(game));
        }
    }

    [HttpDelete("games/{gameId:int}/players/me")]
    public async Task<IActionResult> LeaveGame(int gameId, [FromQuery] string? resolution = null,
        [FromQuery] int? successorId = null)
    {
        if (!TryGetPlayer(out var player)) return Unauthorized();
        if (!Games.TryGetValue(gameId, out var game)) return NotFound();
        GameScoreRequest? scoreRequest = null;
        lock (game)
        {
            if (game.IsCompleting) return Conflict(new { message = "Результаты игры сохраняются. Дождитесь завершения." });
            var member = game.Players.FirstOrDefault(candidate => candidate.Id == player.Id);
            if (member is null || member.HasLeft) return NoContent();
            if (game.Session.State == GameState.Preparation)
            {
                if (game.OwnerId == player.Id) Games.TryRemove(gameId, out _);
                else
                {
                    game.Session.RemovePlayer(player.Id);
                    member.Id = null;
                    member.Name = "Ожидает игрока";
                    member.WaitingForPlayer = true;
                }
                return NoContent();
            }
            var successors = game.Players.Where(candidate => candidate.Id.HasValue &&
                candidate.Id != player.Id && !candidate.IsComputer && !candidate.HasLeft).ToArray();
            if (successors.Length > 0)
            {
                if (resolution != "keep") return Conflict(new { message = "Подтвердите выход с сохранением результатов." });
                if (game.OwnerId == player.Id)
                {
                    var successor = successors.FirstOrDefault(candidate => candidate.Id == successorId);
                    if (successor is null) return Conflict(new { message = "Выберите нового организатора из оставшихся игроков." });
                    game.OwnerId = successor.Id!.Value;
                }
                member.HasLeft = true;
                game.Session.LeavePlayer(player.Id);
                return NoContent();
            }
            if (resolution is not ("discard" or "calculate"))
                return Conflict(new { message = "Выберите: не сохранять результат или рассчитать как есть." });
            if (resolution == "discard")
            {
                if (game.Players.Any(candidate => candidate.HasLeft && !candidate.IsComputer))
                    return Conflict(new { message = "В партии есть результаты ушедших игроков. Их необходимо сохранить." });
                Games.TryRemove(gameId, out _);
                return NoContent();
            }
            scoreRequest = BuildScoreRequest(game);
            game.IsCompleting = true;
        }
        try
        {
            var result = await new GameScoreController(dbContext).Save(scoreRequest);
            if (result is not CreatedAtActionResult) return result;
            lock (game) Games.TryRemove(gameId, out _);
            return Ok(((CreatedAtActionResult)result).Value);
        }
        finally
        {
            lock (game) game.IsCompleting = false;
        }
    }

    [HttpGet("games/{gameId:int}/leave-options")]
    public IActionResult GetLeaveOptions(int gameId)
    {
        if (!TryGetPlayer(out var player)) return Unauthorized();
        if (!Games.TryGetValue(gameId, out var game)) return NotFound();
        lock (game)
        {
            if (!game.Players.Any(member => member.Id == player.Id && !member.HasLeft)) return Forbid();
            var successors = game.Players.Where(member => member.Id.HasValue && member.Id != player.Id &&
                !member.IsComputer && !member.HasLeft).Select(member => new { member.Id, member.Name }).ToArray();
            return Ok(new
            {
                hasOtherPlayers = successors.Length > 0,
                isOwner = game.OwnerId == player.Id,
                canDiscard = !game.Players.Any(member => member.HasLeft && !member.IsComputer),
                successors
            });
        }
    }

    private static GameScoreRequest BuildScoreRequest(LobbyGame game) => new()
    {
        GameType = GameType.Online,
        Players = game.Session.PlayerDecks.Values.Select((deck, index) => new PlayerScoreRequest
        {
            PlayerNumber = index + 1,
            PlayerName = deck.Player.Name,
            Buildings = deck.Buildings.Values.GroupBy(building => building.Type).Select(group => new BuildingScoreRequest
            {
                Building = group.Key, Count = group.Count(), InsideWallCount = group.Count(building => building.IsInsideWall)
            }).ToList(),
            CultureIcons = deck.Culture.Count,
            GuildResources = deck.GameResources.Sum(resource => resource.Count),
            UniversityCultureLevel = deck.Culture.Count,
            DisasterPoints = deck.Skull.Count
        }).ToList()
    };

    [HttpGet("games/{gameId:int}/board")]
    public IActionResult GetBoard(int gameId)
    {
        if (!TryGetPlayer(out var player)) return Unauthorized();
        if (!Games.TryGetValue(gameId, out var game)) return NotFound();
        lock (game)
        {
            if (!game.Session.PlayerDecks.ContainsKey(player.Id) || game.Session.HasLeft(player.Id)) return Forbid();
            return Ok(BoardResponse(game));
        }
    }

    [HttpPost("games/{gameId:int}/actions")]
    public IActionResult Play(int gameId, [FromBody] GameActionRequest request)
    {
        if (!TryGetPlayer(out var player)) return Unauthorized();
        if (!Games.TryGetValue(gameId, out var game)) return NotFound();
        lock (game)
        {
            if (!game.Session.PlayerDecks.TryGetValue(player.Id, out var deck) || game.Session.HasLeft(player.Id)) return Forbid();
            if (game.IsCompleting) return Conflict(new { message = "Игра завершается." });
            if (game.Session.State != MedievalEra.Server.Core.Game.Enums.GameState.Playing)
                return Conflict(new { message = "Игра не находится в состоянии игры." });
            if (request.Action is "next-round" or "scoring" && game.OwnerId != player.Id) return Forbid();
            var dice = deck.Dices.ToArray();
            if (request.Action is "freeze" or "unfreeze" or "extra-roll" &&
                (request.DiceIndex < 0 || request.DiceIndex >= dice.Length ||
                 (request.Action == "extra-roll" && (request.TargetIndex < 0 || request.TargetIndex >= dice.Length))))
                return BadRequest("Неверный номер кубика.");
            try
            {
                switch (request.Action)
                {
                    case "roll": game.Session.Roll(player.Id); break;
                    case "finish": game.Session.FinishTurn(player.Id); break;
                    case "freeze": deck.Freeze(dice[request.DiceIndex]); break;
                    case "unfreeze": deck.Unfreeze(dice[request.DiceIndex]); break;
                    case "extra-roll": deck.ExtraRoll(dice[request.DiceIndex], dice[request.TargetIndex]); break;
                    case "next-round": game.Session.StartNextRound(); break;
                    case "scoring": game.Session.BeginScoring(); game.Status = "scoring"; break;
                    default: return BadRequest("Неизвестное действие.");
                }
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(new { message = exception.Message });
            }
            return Ok(BoardResponse(game));
        }
    }

    private static object BoardResponse(LobbyGame game) => new
    {
        game.Id,
        game.Session.State,
        game.Session.RoundNumber,
        buildingSupply = game.Session.BuildingSupply.Available.Select(building => new
        {
            building.Type, building.Name, building.Score, building.DiceType,
            mask = building.Mask.ToString()
        }).ToArray(),
        players = game.Session.PlayerDecks.Values.Select(deck => new
        {
            deck.Player,
            hasLeft = game.Session.HasLeft(deck.Player.Id),
            deck.RestRolls,
            deck.IsTurnCompleted,
            resources = deck.GameResources.Concat([deck.Skull, deck.Culture])
                .Select(resource => new { resource.Resource, resource.Count, resource.MaxValue }).ToArray(),
            field = Enumerable.Range(0, MedievalEra.Server.Core.Game.Deck.PlayerDeck.BoardSize)
                .Select(y => Enumerable.Range(0, MedievalEra.Server.Core.Game.Deck.PlayerDeck.BoardSize)
                    .Select(x => deck.Field[x, y]).ToArray()).ToArray(),
            buildings = deck.Buildings.Select(entry => new
            {
                id = entry.Key, entry.Value.Type, entry.Value.Name,
                mask = entry.Value.Mask.ToString(), entry.Value.Rotation, entry.Value.IsInsideWall
            }).ToArray(),
            dice = deck.Dices.Select((dice, index) => new
            {
                index, dice.Name, dice.DiceType,
                face = deck.CurrentFaces.TryGetValue(dice, out var face)
                    ? new { face.DiceType, values = new Dictionary<MedievalEra.Server.Core.Game.Enums.DiceResource, int>(face.Values), face.Choose }
                    : null,
                frozen = deck.FreezedDices.Contains(dice)
            }).ToArray()
        }).ToArray()
    };

    public sealed class GameActionRequest
    {
        public string Action { get; set; } = "";
        public int DiceIndex { get; set; } = -1;
        public int TargetIndex { get; set; } = -1;
    }

    private bool TryGetPlayer(out LobbyPlayer player)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = User.FindFirstValue(ClaimTypes.Name);
        if (int.TryParse(id, out var playerId) && !string.IsNullOrWhiteSpace(name))
        {
            player = new LobbyPlayer(playerId, name);
            return true;
        }
        player = null!;
        return false;
    }

    private static object ToResponse(LobbyGame game) => new
    {
        game.Id,
        game.Version,
        game.Mode,
        game.AllowBots,
        game.Status,
        state = game.Session.State,
        roundNumber = game.Session.RoundNumber,
        game.CreatedAt,
        game.StartedAt,
        game.OwnerId,
        maxPlayers = game.Players.Count,
        players = game.Players.Select(player => new { Id = player.HasLeft ? (int?)null : player.Id, player.Name, isComputer = player.IsComputer, isWaiting = player.WaitingForPlayer, hasLeft = player.HasLeft })
    };

    private static void CleanupExpiredGames()
    {
        var cutoff = DateTimeOffset.UtcNow - WaitingLifetime;
        foreach (var game in Games.Values.Where(game => game.Status == "waiting" && game.CreatedAt < cutoff))
            Games.TryRemove(game.Id, out _);
    }

    private static void CleanupExpiredPresence()
    {
        var cutoff = DateTimeOffset.UtcNow - TimeSpan.FromSeconds(20);
        foreach (var item in Presence.Where(item => item.Value.LastSeen < cutoff))
            Presence.TryRemove(item.Key, out _);
    }

    public sealed class CreateLobbyGameRequest
    {
        public string Version { get; set; } = "base";
        public string Mode { get; set; } = "players";
        public int OpponentsCount { get; set; } = 1;
        public bool AllowBots { get; set; }
    }
    private sealed record LobbyPlayer(int Id, string Name);
    private sealed record LobbyPresence(LobbyPlayer Player, DateTimeOffset LastSeen);
    private sealed class LobbyMember(int? id, string name, bool waitingForPlayer = false, bool isComputer = false)
    {
        public int? Id { get; set; } = id;
        public string Name { get; set; } = name;
        public bool WaitingForPlayer { get; set; } = waitingForPlayer;
        public bool IsComputer { get; set; } = isComputer;
        public bool HasLeft { get; set; }
    }
    private sealed class LobbyGame(int id, string version, string mode, bool allowBots, DiceFactory diceFactory)
    {
        public Game Session { get; } = new(id, diceFactory);
        public bool IsCompleting { get; set; }
        public int Id { get; } = id;
        public string Version { get; } = version;
        public string Mode { get; } = mode;
        public bool AllowBots { get; } = allowBots;
        public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? StartedAt { get; set; }
        public int OwnerId { get; set; }
        public string Status { get; set; } = "waiting";
        public List<LobbyMember> Players { get; } = [];
    }
}
