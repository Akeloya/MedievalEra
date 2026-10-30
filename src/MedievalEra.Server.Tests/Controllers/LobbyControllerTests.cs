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
    [Test]
    public async Task MovingRequiresMembershipAndUnlockedBuildingAndPreservesStock()
    {
        await using var db = new AppDbContext(AppDbContext.DatabaseType.InMemory, Guid.NewGuid().ToString());
        var owner = Controller(73001, db);
        var outsider = Controller(73002, db);
        var created = (OkObjectResult)owner.CreateGame(new LobbyController.CreateLobbyGameRequest { Mode = "sir-philipp" });
        var id = JsonSerializer.SerializeToElement(created.Value).GetProperty("Id").GetInt32();
        try
        {
            var before = JsonSerializer.SerializeToElement(((OkObjectResult)owner.GetBoard(id)).Value);
            var buildingId = before.GetProperty("players")[0].GetProperty("field")[7][4].GetInt32();
            var request = new LobbyController.MoveBuildingRequest { X = 0, Y = 0, Rotation = 90 };
            Assert.That(outsider.MoveBuilding(id, buildingId, request), Is.InstanceOf<ForbidResult>());
            Assert.That(owner.MoveBuilding(id, 0, request), Is.InstanceOf<ConflictObjectResult>());
            var moved = owner.MoveBuilding(id, buildingId, request);
            Assert.That(moved, Is.InstanceOf<OkObjectResult>());
            var board = JsonSerializer.SerializeToElement(((OkObjectResult)moved).Value);
            var field = board.GetProperty("players")[0].GetProperty("field");
            Assert.That(field[0][0].GetInt32(), Is.EqualTo(buildingId));
            Assert.That(field[1][0].GetInt32(), Is.EqualTo(buildingId));
            Assert.That(field[7][4].ValueKind, Is.EqualTo(JsonValueKind.Null));
            Assert.That(board.GetProperty("buildingSupply").GetArrayLength(), Is.EqualTo(before.GetProperty("buildingSupply").GetArrayLength()));
            Assert.That(board.GetProperty("history")[0].GetProperty("Action").GetString(), Is.EqualTo("move-building"));
            request.X = 6; request.Y = 6;
            Assert.That(owner.MoveBuilding(id, buildingId, request), Is.InstanceOf<ConflictObjectResult>());
            Assert.That(owner.Play(id, new LobbyController.GameActionRequest { Action = "finish-construction" }), Is.InstanceOf<OkObjectResult>());
            request.X = 1; request.Y = 0;
            Assert.That(owner.MoveBuilding(id, buildingId, request), Is.InstanceOf<ConflictObjectResult>());
            var after = JsonSerializer.SerializeToElement(((OkObjectResult)owner.GetBoard(id)).Value);
            Assert.That(after.GetProperty("players")[0].GetProperty("field")[0][0].GetInt32(), Is.EqualTo(buildingId));
            Assert.That(after.GetProperty("history").GetArrayLength(), Is.EqualTo(2));
        }
        finally { await owner.LeaveGame(id, "discard"); }
    }

    [Test]
    public async Task PlacementUsesRequestedWallShapeAndRejectsOverlapAndNonMembers()
    {
        await using var db = new AppDbContext(AppDbContext.DatabaseType.InMemory, Guid.NewGuid().ToString());
        var owner = Controller(72001, db);
        var outsider = Controller(72002, db);
        var created = (OkObjectResult)owner.CreateGame(new LobbyController.CreateLobbyGameRequest { Mode = "sir-philipp" });
        var id = JsonSerializer.SerializeToElement(created.Value).GetProperty("Id").GetInt32();
        try
        {
            var request = new LobbyController.PlaceBuildingRequest
            {
                Type = MedievalEra.Server.Core.Game.Enums.Buildings.Wall,
                Mask = "xxxx", X = 0, Y = 0, Rotation = 90
            };
            Assert.That(outsider.PlaceBuilding(id, request), Is.InstanceOf<ForbidResult>());
            var placed = owner.PlaceBuilding(id, request);
            Assert.That(placed, Is.InstanceOf<OkObjectResult>());
            var board = JsonSerializer.SerializeToElement(((OkObjectResult)placed).Value);
            Assert.That(board.GetProperty("buildingCatalog").GetArrayLength(), Is.EqualTo(17));
            var history = board.GetProperty("history");
            Assert.That(history.GetArrayLength(), Is.EqualTo(1));
            Assert.That(history[0].GetProperty("PlayerId").GetInt32(), Is.EqualTo(72001));
            Assert.That(history[0].GetProperty("Action").GetString(), Is.EqualTo("build"));
            var field = board.GetProperty("players")[0].GetProperty("field");
            var placedId = field[0][0].GetInt32();
            for (var y = 0; y < 4; y++) Assert.That(field[y][0].GetInt32(), Is.EqualTo(placedId));
            Assert.That(history[0].GetProperty("Description").GetString(), Does.Not.Contain("°"));
            Assert.That(owner.PlaceBuilding(id, request), Is.InstanceOf<ConflictObjectResult>());
            request.X = 13;
            request.Y = 13;
            Assert.That(owner.PlaceBuilding(id, request), Is.InstanceOf<ConflictObjectResult>());
            var after = JsonSerializer.SerializeToElement(((OkObjectResult)owner.GetBoard(id)).Value);
            Assert.That(after.GetProperty("buildingSupply").GetArrayLength(), Is.EqualTo(board.GetProperty("buildingSupply").GetArrayLength()));
            Assert.That(after.GetProperty("history").GetArrayLength(), Is.EqualTo(1));
            var rolled = owner.Play(id, new LobbyController.GameActionRequest { Action = "roll" });
            Assert.That(rolled, Is.InstanceOf<OkObjectResult>());
            var rolledHistory = JsonSerializer.SerializeToElement(((OkObjectResult)rolled).Value).GetProperty("history");
            Assert.That(rolledHistory.GetArrayLength(), Is.EqualTo(2));
            Assert.That(rolledHistory[1].GetProperty("Action").GetString(), Is.EqualTo("roll"));
            Assert.That(rolledHistory[1].GetProperty("Dice").GetArrayLength(), Is.GreaterThan(0));
            Assert.That(rolledHistory[1].GetProperty("Dice")[0].GetProperty("Values").EnumerateObject().Any(), Is.True);
            Assert.That(outsider.RemoveBuilding(id, placedId), Is.InstanceOf<ForbidResult>());
            Assert.That(owner.RemoveBuilding(id, 0), Is.InstanceOf<ConflictObjectResult>());
            var removed = owner.RemoveBuilding(id, placedId);
            Assert.That(removed, Is.InstanceOf<OkObjectResult>());
            var removedBoard = JsonSerializer.SerializeToElement(((OkObjectResult)removed).Value);
            Assert.That(removedBoard.GetProperty("players")[0].GetProperty("AllowToRemoveBuilding").GetBoolean(), Is.True);
            for (var y = 0; y < 4; y++)
                Assert.That(removedBoard.GetProperty("players")[0].GetProperty("field")[y][0].ValueKind, Is.EqualTo(JsonValueKind.Null));
            Assert.That(removedBoard.GetProperty("buildingSupply").GetArrayLength(), Is.EqualTo(board.GetProperty("buildingSupply").GetArrayLength() + 1));
            Assert.That(removedBoard.GetProperty("history")[2].GetProperty("Action").GetString(), Is.EqualTo("remove-building"));
            Assert.That(owner.RemoveBuilding(id, placedId), Is.InstanceOf<NotFoundResult>());
        }
        finally { await owner.LeaveGame(id, "discard"); }
    }

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
