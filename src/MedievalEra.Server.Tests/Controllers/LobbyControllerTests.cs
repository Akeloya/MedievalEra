using System.Security.Claims;
using System.Text.Json;
using MedievalEra.Server.Controllers;
using MedievalEra.Server.Core.Game.Common;
using MedievalEra.Server.Core.Game.Dice;
using MedievalEra.Server.Core.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MedievalEra.Server.Tests.Controllers;

public class LobbyControllerTests
{
    private static LobbyController Controller(int id, AppDbContext db) => new(new DiceFactory(new DefaultRandomProvider()), db)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                    new Claim(ClaimTypes.Name, $"Player {id}")
                }, "test"))
            }
        }
    };

    [TestCase(true)]
    [TestCase(false)]
    public async Task LeavingMultiplayerKeepsPartyAndTransfersOwnership(bool ownerLeaves)
    {
        await using var db = new AppDbContext(AppDbContext.DatabaseType.InMemory, Guid.NewGuid().ToString());
        var ownerId = ownerLeaves ? 71001 : 71003;
        var guestId = ownerLeaves ? 71002 : 71004;
        var owner = Controller(ownerId, db);
        var guest = Controller(guestId, db);
        var created = (OkObjectResult)owner.CreateGame(new LobbyController.CreateLobbyGameRequest());
        var id = JsonSerializer.SerializeToElement(created.Value).GetProperty("Id").GetInt32();
        try
        {
            Assert.That(guest.JoinGame(id), Is.InstanceOf<OkObjectResult>());
            Assert.That(owner.StartGame(id), Is.InstanceOf<OkObjectResult>());
            var leaving = ownerLeaves ? owner : guest;
            var staying = ownerLeaves ? guest : owner;
            Assert.That(await leaving.LeaveGame(id), Is.InstanceOf<ConflictObjectResult>());
            Assert.That(await leaving.LeaveGame(id, "keep", ownerLeaves ? guestId : null), Is.InstanceOf<NoContentResult>());
            Assert.That(leaving.GetBoard(id), Is.InstanceOf<ForbidResult>());
            var board = (OkObjectResult)staying.GetBoard(id);
            Assert.That(JsonSerializer.SerializeToElement(board.Value).GetProperty("players").GetArrayLength(), Is.EqualTo(2));
            var options = JsonSerializer.SerializeToElement(((OkObjectResult)staying.GetLeaveOptions(id)).Value);
            Assert.That(options.GetProperty("isOwner").GetBoolean(), Is.True);
            Assert.That(options.GetProperty("canDiscard").GetBoolean(), Is.False);
            Assert.That(await staying.LeaveGame(id, "discard"), Is.InstanceOf<ConflictObjectResult>());
            Assert.That(await staying.LeaveGame(id, "calculate"), Is.InstanceOf<OkObjectResult>());
            Assert.That(await db.GameScores.CountAsync(), Is.EqualTo(1));
            Assert.That(await db.PlayerScores.CountAsync(), Is.EqualTo(2));
        }
        finally
        {
            await owner.LeaveGame(id, "calculate");
            await guest.LeaveGame(id, "calculate");
        }
    }

    [TestCase("sir-philipp", "discard")]
    [TestCase("sir-philipp", "calculate")]
    [TestCase("players", "discard")]
    [TestCase("players", "calculate")]
    public async Task SoloExitRequiresDecisionAndSavesOnlyWhenRequested(string mode, string resolution)
    {
        await using var db = new AppDbContext(AppDbContext.DatabaseType.InMemory, Guid.NewGuid().ToString());
        var owner = Controller(72000 + (mode == "players" ? 10 : 0) + (resolution == "calculate" ? 1 : 0), db);
        var created = (OkObjectResult)owner.CreateGame(new LobbyController.CreateLobbyGameRequest { Mode = mode, AllowBots = true });
        var id = JsonSerializer.SerializeToElement(created.Value).GetProperty("Id").GetInt32();
        try
        {
            if (mode == "players") Assert.That(owner.StartGame(id, true), Is.InstanceOf<OkObjectResult>());
            Assert.That(await owner.LeaveGame(id), Is.InstanceOf<ConflictObjectResult>());
            Assert.That(owner.GetBoard(id), Is.InstanceOf<OkObjectResult>());
            var result = await owner.LeaveGame(id, resolution);
            Assert.That(result, resolution == "calculate" ? Is.InstanceOf<OkObjectResult>() : Is.InstanceOf<NoContentResult>());
            Assert.That(owner.GetBoard(id), Is.InstanceOf<NotFoundResult>());
            Assert.That(await db.GameScores.CountAsync(), Is.EqualTo(resolution == "calculate" ? 1 : 0));
        }
        finally { await owner.LeaveGame(id, "discard"); }
    }
}
