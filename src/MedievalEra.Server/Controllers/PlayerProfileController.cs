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
                    result.PlayerName,
                    result.Title,
                    result.HasTitle,
                    result.FinalTotal,
                    GameId = result.GameScoreId,
                    result.GameScore.CreatedAt,
                    result.GameScore.GameType
                })
                .OrderByDescending(result => result.CreatedAt)
                .ToListAsync();

            return Ok(new { profile.Name, PendingResults = pendingResults });
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
}
