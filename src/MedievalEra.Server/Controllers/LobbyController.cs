using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedievalEra.Server.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public sealed class LobbyController : ControllerBase
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
            .SelectMany(game => game.Players.Where(member => member.Id.HasValue).Select(member => member.Id!.Value))
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

        var existing = Games.Values.FirstOrDefault(game => game.Players.Any(member => member.Id == player.Id));
        if (existing is not null) return Conflict(new { message = "Вы уже зарегистрированы в игре.", gameId = existing.Id });

        var game = new LobbyGame(Interlocked.Increment(ref _nextGameId), request.Version, request.Mode, request.AllowBots);
        game.OwnerId = player.Id;
        game.Players.Add(new LobbyMember(player.Id, player.Name));
        if (request.Mode == "sir-philipp")
        {
            game.Players.Add(new LobbyMember(null, "Сэр Филипп", false, true));
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
            if (Games.Values.Any(candidate => candidate.Players.Any(member => member.Id == player.Id)))
                return Conflict(new { message = "Вы уже зарегистрированы в другой игре." });
            var openSeat = game.Players.FirstOrDefault(member => member.WaitingForPlayer);
            if (openSeat is null) return Conflict(new { message = "В этой игре нет свободного места для игрока." });
            openSeat.Id = player.Id;
            openSeat.Name = player.Name;
            openSeat.WaitingForPlayer = false;
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
            game.Status = "playing";
            game.StartedAt = DateTimeOffset.UtcNow;
            return Ok(ToResponse(game));
        }
    }

    [HttpDelete("games/{gameId:int}/players/me")]
    public IActionResult LeaveGame(int gameId)
    {
        if (!TryGetPlayer(out var player)) return Unauthorized();
        if (!Games.TryGetValue(gameId, out var game)) return NotFound();
        lock (game)
        {
            if (game.OwnerId == player.Id)
            {
                Games.TryRemove(gameId, out _);
                return NoContent();
            }
            var member = game.Players.FirstOrDefault(candidate => candidate.Id == player.Id);
            if (member is not null)
            {
                member.Id = null;
                member.Name = "Ожидает игрока";
                member.WaitingForPlayer = true;
            }
        }
        return NoContent();
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
        game.CreatedAt,
        game.StartedAt,
        game.OwnerId,
        maxPlayers = game.Players.Count,
        players = game.Players.Select(player => new { player.Id, player.Name, isComputer = player.IsComputer, isWaiting = player.WaitingForPlayer })
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
    }
    private sealed class LobbyGame(int id, string version, string mode, bool allowBots)
    {
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
