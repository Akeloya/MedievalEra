using System.Security.Claims;

using MedievalEra.Server.Core.Store;
using MedievalEra.Server.Core.Store.Entitites;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedievalEra.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlayerProfileController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IPasswordHasher<PlayerEntity> _passwordHasher;

        public PlayerProfileController(
            AppDbContext dbContext,
            IPasswordHasher<PlayerEntity> passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] ProfileCredentials request)
        {
            var name = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name) || name.Length > 50 ||
                string.IsNullOrWhiteSpace(request.Password) ||
                request.Password.Length < 8 || request.Password.Length > 128)
            {
                return BadRequest("Укажите имя не длиннее 50 символов и пароль длиной от 8 до 128 символов.");
            }

            var profiles = await _dbContext.Players.ToListAsync();
            var profile = profiles.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase));
            if (profile?.PasswordHash is not null)
            {
                return Conflict("Профиль с таким именем уже зарегистрирован.");
            }

            if (profile is null)
            {
                profile = new PlayerEntity { Name = name, CreatedAt = DateTime.UtcNow };
                _dbContext.Players.Add(profile);
            }

            profile.PasswordHash = _passwordHasher.HashPassword(profile, request.Password);
            await _dbContext.SaveChangesAsync();
            await SignInAsync(profile);

            return Ok(new { profile.Name });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ProfileCredentials request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrEmpty(request.Password) ||
                request.Password.Length > 128)
            {
                return Unauthorized("Неверное имя игрока или пароль.");
            }

            var profile = (await _dbContext.Players.ToListAsync()).FirstOrDefault(candidate =>
                string.Equals(candidate.Name, request.Name?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (profile?.PasswordHash is null ||
                _passwordHasher.VerifyHashedPassword(profile, profile.PasswordHash, request.Password ?? string.Empty)
                    == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Неверное имя игрока или пароль.");
            }

            await SignInAsync(profile);
            return Ok(new { profile.Name });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentProfile()
        {
            if (!TryGetProfileId(out var profileId))
            {
                return Unauthorized();
            }

            var profile = await _dbContext.Players
                .AsNoTracking()
                .SingleOrDefaultAsync(player => player.Id == profileId);
            if (profile is null)
            {
                return Unauthorized();
            }

            var pendingResults = await _dbContext.PlayerScores
                .AsNoTracking()
                .Where(result => result.PlayerEntityId == profileId && !result.ResultAccepted)
                .Select(result => new
                {
                    result.Id,
                    PlayerName = profile.Name,
                    result.Title,
                    result.HasTitle,
                    result.FinalTotal,
                    GameId = result.GameScoreId,
                    result.GameScore.CreatedAt,
                    result.GameScore.GameType
                })
                .OrderByDescending(result => result.CreatedAt)
                .ToListAsync();

            var titleHistory = await _dbContext.PlayerScores
                .AsNoTracking()
                .Where(result => result.PlayerEntityId == profileId)
                .Select(result => new { result.GameScoreId, result.Title, result.HasTitle })
                .ToListAsync();
            var titleCounts = titleHistory
                .GroupBy(result => result.Title)
                .Select(group => new
                {
                    Title = group.Key,
                    Count = group.Select(result => result.GameScoreId).Distinct().Count()
                })
                .OrderByDescending(result => result.Count)
                .ThenBy(result => result.Title)
                .ToList();
            var gameCount = titleHistory.Select(result => result.GameScoreId).Distinct().Count();
            var winCount = titleHistory
                .Where(result => result.HasTitle)
                .Select(result => result.GameScoreId)
                .Distinct()
                .Count();
            var statistics = new
            {
                GamesPlayed = gameCount,
                Wins = winCount,
                MostCommonTitle = titleCounts.FirstOrDefault()?.Title,
                Titles = titleCounts
            };

            var recentResults = await _dbContext.PlayerScores
                .AsNoTracking()
                .Where(result => result.PlayerEntityId == profileId)
                .OrderByDescending(result => result.GameScore.CreatedAt)
                .ThenByDescending(result => result.Id)
                .Take(5)
                .Select(result => new
                {
                    result.Id,
                    GameId = result.GameScoreId,
                    result.GameScore.CreatedAt,
                    result.GameScore.GameType,
                    result.Title,
                    result.TitleComment,
                    result.FinalTotal,
                    result.HasTitle,
                    result.ResultAccepted,
                    Buildings = result.Buildings.Select(building => new
                    {
                        building.Building,
                        building.Count,
                        building.InsideWallCount
                    })
                })
                .ToListAsync();

            var buildingNames = MedievalEra.Server.Core.Game.BuildingsFactory.GameSet
                .ToDictionary(building => building.Type, building => building.Name);
            var recentResultsWithBuildingNames = recentResults.Select(result => new
            {
                result.Id,
                result.GameId,
                result.CreatedAt,
                result.GameType,
                result.Title,
                result.TitleComment,
                result.FinalTotal,
                result.HasTitle,
                result.ResultAccepted,
                Buildings = result.Buildings
                    .Where(building => building.Count + building.InsideWallCount > 0)
                    .Select(building => new
                    {
                        Name = buildingNames[building.Building],
                        building.Count,
                        building.InsideWallCount
                    })
            });

            return Ok(new
            {
                profile.Name,
                PendingResults = pendingResults,
                RecentResults = recentResultsWithBuildingNames,
                Statistics = statistics
            });
        }

        [HttpGet("name-status")]
        public async Task<IActionResult> GetNameStatus([FromQuery] string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Ok(new { status = "unknown" });
            }

            var matchingProfile = (await _dbContext.Players.AsNoTracking().ToListAsync())
                .FirstOrDefault(profile => string.Equals(
                    profile.Name,
                    name.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            var status = matchingProfile is null
                ? "unknown"
                : matchingProfile.PasswordHash is null ? "offline" : "registered";
            return Ok(new { status });
        }

        [Authorize]
        [HttpPost("change-name")]
        public async Task<IActionResult> ChangeName([FromBody] ChangeProfileNameRequest request)
        {
            if (!TryGetProfileId(out var profileId))
            {
                return Unauthorized();
            }

            var profile = await _dbContext.Players.SingleOrDefaultAsync(player => player.Id == profileId);
            if (profile?.PasswordHash is null ||
                _passwordHasher.VerifyHashedPassword(profile, profile.PasswordHash, request.CurrentPassword ?? string.Empty)
                    == PasswordVerificationResult.Failed)
            {
                return BadRequest("Неверный текущий пароль.");
            }

            var name = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name) || name.Length > 50)
            {
                return BadRequest("Укажите имя игрока длиной не более 50 символов.");
            }

            var hasGameResults = await _dbContext.PlayerScores.AnyAsync(result => result.PlayerEntityId == profileId);
            var conflictingProfile = (await _dbContext.Players
                    .Where(player => player.Id != profileId)
                    .ToListAsync())
                .SingleOrDefault(player => string.Equals(
                    player.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase));

            if (conflictingProfile is not null)
            {
                if (conflictingProfile.PasswordHash is not null)
                {
                    return Conflict(new { code = "name_taken", message = "Это имя уже занято другим игроком." });
                }

                var offlineResults = await _dbContext.PlayerScores
                    .Where(result => result.PlayerEntityId == conflictingProfile.Id)
                    .ToListAsync();
                var offlineGameCount = offlineResults
                    .Select(result => result.GameScoreId)
                    .Distinct()
                    .Count();

                if (hasGameResults || offlineResults.Count == 0)
                {
                    return Conflict(new { code = "name_taken", message = "Это имя уже занято другим игроком." });
                }

                if (!request.AcceptOfflineResults)
                {
                    return Conflict(new
                    {
                        code = "offline_results_available",
                        message = $"Под именем «{name}» обнаружены результаты проведённых игр ({offlineGameCount}). Принять их и привязать к вашему профилю?",
                        gamesCount = offlineGameCount
                    });
                }

                Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
                try
                {
                    if (_dbContext.Database.IsRelational())
                    {
                        transaction = await _dbContext.Database.BeginTransactionAsync();
                    }

                    foreach (var result in offlineResults)
                    {
                        result.PlayerEntityId = profileId;
                        result.PlayerName = name;
                    }

                    await _dbContext.SaveChangesAsync();
                    _dbContext.Players.Remove(conflictingProfile);
                    await _dbContext.SaveChangesAsync();

                    profile.Name = name;
                    await _dbContext.SaveChangesAsync();
                    if (transaction is not null)
                    {
                        await transaction.CommitAsync();
                    }
                }
                catch (DbUpdateException)
                {
                    return Conflict(new { code = "name_taken", message = "Это имя уже занято другим игроком." });
                }
                finally
                {
                    if (transaction is not null)
                    {
                        await transaction.DisposeAsync();
                    }
                }
            }
            else
            {
                var profileResults = await _dbContext.PlayerScores
                    .Where(result => result.PlayerEntityId == profileId)
                    .ToListAsync();
                foreach (var result in profileResults)
                {
                    result.PlayerName = name;
                }

                profile.Name = name;
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    return Conflict(new { code = "name_taken", message = "Это имя уже занято другим игроком." });
                }
            }

            await SignInAsync(profile);
            return Ok(new { profile.Name });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangeProfilePasswordRequest request)
        {
            if (!TryGetProfileId(out var profileId))
            {
                return Unauthorized();
            }

            var profile = await _dbContext.Players.SingleOrDefaultAsync(player => player.Id == profileId);
            if (profile?.PasswordHash is null ||
                _passwordHasher.VerifyHashedPassword(profile, profile.PasswordHash, request.CurrentPassword ?? string.Empty)
                    == PasswordVerificationResult.Failed)
            {
                return BadRequest("Неверный текущий пароль.");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword) ||
                request.NewPassword.Length < 8 || request.NewPassword.Length > 128)
            {
                return BadRequest("Новый пароль должен содержать от 8 до 128 символов.");
            }

            profile.PasswordHash = _passwordHasher.HashPassword(profile, request.NewPassword);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpPost("results/{resultId:int}/accept")]
        public async Task<IActionResult> AcceptResult(int resultId)
        {
            if (!TryGetProfileId(out var profileId))
            {
                return Unauthorized();
            }

            var result = await _dbContext.PlayerScores.SingleOrDefaultAsync(playerScore =>
                playerScore.Id == resultId && playerScore.PlayerEntityId == profileId);
            if (result is null)
            {
                return NotFound();
            }

            result.ResultAccepted = true;
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        private async Task SignInAsync(PlayerEntity profile)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, profile.Id.ToString()),
                new Claim(ClaimTypes.Name, profile.Name)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }

        private bool TryGetProfileId(out int profileId) =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out profileId);
    }

    public class ProfileCredentials
    {
        public string? Name { get; set; }
        public string? Password { get; set; }
    }

    public class ChangeProfileNameRequest
    {
        public string? Name { get; set; }
        public string? CurrentPassword { get; set; }
        public bool AcceptOfflineResults { get; set; }
    }

    public class ChangeProfilePasswordRequest
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
