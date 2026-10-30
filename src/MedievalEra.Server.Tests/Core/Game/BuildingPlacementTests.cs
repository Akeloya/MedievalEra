using MedievalEra.Server.Core.Game;
using MedievalEra.Server.Core.Game.Common;
using MedievalEra.Server.Core.Game.Deck;
using MedievalEra.Server.Core.Game.Dice;
using MedievalEra.Server.Core.Game.Enums;

namespace MedievalEra.Server.Tests.Core.Game;

[TestFixture]
public class BuildingPlacementTests
{
    private static Building Create(string mask = "x") =>
        new(Buildings.Wall, "Стена", null, 1, DiceType.None, mask);

    private static PlayerDeck Deck() => new("Player", new DiceFactory(new DefaultRandomProvider()));

    [TestCase("")]
    [TestCase("oo")]
    [TestCase("xx\nx")]
    [TestCase("x ")]
    [TestCase("X")]
    [TestCase("xxх")]
    [TestCase("x\n")]
    public void InvalidMasksAreRejected(string mask) =>
        Assert.Throws<ArgumentException>(() => new BuildingMask(mask));

    [Test]
    public void FactoryCreatesFarmWithThreeColumnsAndTwoRows()
    {
        var farm = new BuildingsFactory().Get(Buildings.Farm);
        Assert.That(farm.Mask.ToString(), Is.EqualTo("xxx\nxxx"));
        Assert.That(farm.Masks.Count, Is.EqualTo(4));
    }

    [Test]
    public void RectangularMaskHasFourClockwiseRotations()
    {
        var building = Create("xoo\nxxx");
        Assert.That(building.Masks.Select(m => m.ToString()), Is.EqualTo(new[]
        {
            "xoo\nxxx", "xx\nxo\nxo", "xxx\noox", "ox\nox\nxx"
        }));
        for (var i = 0; i < 4; i++) building.Rotate();
        Assert.That(building.Rotation, Is.Zero);
    }

    [Test]
    public void PlacementAllowsAnotherBuildingInHoleAndRemovesOnlySelectedInstance()
    {
        var deck = Deck();
        var arch = Create("xxx\nxox\nxox");
        var small = Create();
        var supply = new BuildingSupply([arch, small]);
        Assert.That(deck.TakeBuilding(supply, arch), Is.True);
        Assert.That(deck.TakeBuilding(supply, small), Is.True);
        Assert.That(deck.Build(0, 0, arch), Is.True);
        Assert.That(deck.Build(1, 1, small), Is.True);
        Assert.That(deck.Field[0, 0], Is.EqualTo(1));
        Assert.That(deck.Field[1, 1], Is.EqualTo(2));
        Assert.That(deck.RemoveBuilding(1), Is.True);
        Assert.That(deck.Field[0, 0], Is.Null);
        Assert.That(deck.Field[1, 1], Is.EqualTo(2));
        Assert.That(deck.UnbuiltBuildings, Does.Contain(arch));
        Assert.That(deck.Build(3, 0, arch), Is.True);
        Assert.That(deck.Field[3, 0], Is.EqualTo(3));
    }

    [Test]
    public void PlacementChecksInventoryBoundsRotationAndCollisionsWithoutPartialWrites()
    {
        var deck = Deck();
        var wall = Create("xx");
        var supply = new BuildingSupply([wall]);
        Assert.That(deck.Build(0, 0, wall), Is.False);
        deck.TakeBuilding(supply, wall);
        Assert.That(deck.CanBuild(-1, 0, wall), Is.False);
        Assert.That(deck.CanBuild(int.MaxValue, 0, wall), Is.False);
        Assert.That(deck.CanBuild(13, 0, wall), Is.False);
        Assert.That(deck.Build(5, 7, wall), Is.False);
        Assert.That(deck.Field[5, 7], Is.Null);
        wall.Rotate();
        Assert.That(deck.Build(13, 0, wall), Is.True);
        Assert.That(deck.Field[13, 1], Is.EqualTo(1));
        Assert.That(deck.Build(0, 0, wall), Is.False);
        Assert.Throws<InvalidOperationException>(() => wall.Rotate());
        Assert.That(supply.Return(wall), Is.False);
        Assert.That(deck.RemoveBuilding(1), Is.True);
        Assert.DoesNotThrow(() => wall.Rotate());
    }

    [Test]
    public void StartingTowerAndProtectedBuildingsCannotBeRemoved()
    {
        var deck = Deck();
        Assert.That(deck.Field[7, 7], Is.EqualTo(0));
        Assert.That(deck.Buildings[0].Type, Is.EqualTo(Buildings.Tower));
        Assert.That(deck.Buildings[0].Mask.ToString(), Is.EqualTo("xx\nxx"));
        Assert.That(deck.Field[6, 7], Is.EqualTo(0));
        Assert.That(deck.Field[7, 6], Is.EqualTo(0));
        Assert.That(deck.Field[6, 6], Is.EqualTo(0));
        var occupiedCells = deck.Field.Cast<int?>().Count(id => id == 0);
        Assert.That(occupiedCells, Is.EqualTo(4));
        Assert.That(deck.RemoveBuilding(0), Is.False);
        var wall = Create();
        var supply = new BuildingSupply([wall]);
        deck.TakeBuilding(supply, wall);
        deck.Build(0, 0, wall);
        wall.IsInsideWall = true;
        Assert.That(deck.RemoveBuilding(1), Is.False);
        var field = deck.Field;
        field[0, 0] = null;
        Assert.That(deck.Field[0, 0], Is.EqualTo(1));
    }

    [Test]
    public void MovingPreservesIdentityAndStockAndAllowsOverlapWithOwnCells()
    {
        var deck = Deck();
        var building = Create("xxx");
        var supply = new BuildingSupply([building]);
        Assert.That(deck.BuildFromSupply(supply, building, 0, 0, 0), Is.True);
        Assert.That(deck.MoveBuilding(1, 1, 0, 0), Is.True);
        Assert.That(deck.Field[0, 0], Is.Null);
        Assert.That(deck.Field[1, 0], Is.EqualTo(1));
        Assert.That(deck.Field[3, 0], Is.EqualTo(1));
        Assert.That(deck.MoveBuilding(1, 1, 0, 90), Is.True);
        Assert.That(deck.Field[2, 0], Is.Null);
        Assert.That(deck.Field[1, 2], Is.EqualTo(1));
        Assert.That(deck.Buildings[1], Is.SameAs(building));
        Assert.That(building.Rotation, Is.EqualTo(90));
        Assert.That(supply.Available, Is.Empty);
        Assert.That(deck.UnbuiltBuildings, Is.Empty);
        Assert.That(deck.RemoveBuilding(1), Is.True);
        Assert.That(deck.Field[1, 2], Is.Null);
    }

    [Test]
    public void RejectedMovesPreserveOriginalCellsRotationAndInventory()
    {
        var deck = Deck();
        var building = Create("xx");
        var supply = new BuildingSupply([building]);
        deck.BuildFromSupply(supply, building, 0, 0, 0);
        Assert.That(deck.MoveBuilding(1, 6, 6, 90), Is.False);
        Assert.That(deck.MoveBuilding(1, 13, 13, 90), Is.False);
        Assert.That(deck.MoveBuilding(1, -1, 0, 0), Is.False);
        Assert.That(deck.MoveBuilding(1, 0, 0, 45), Is.False);
        Assert.That(deck.MoveBuilding(99, 0, 0, 0), Is.False);
        building.Locked = true;
        Assert.That(deck.MoveBuilding(1, 3, 0, 90), Is.False);
        Assert.That(deck.MoveBuilding(0, 3, 0, 0), Is.False);
        Assert.That(deck.Field[0, 0], Is.EqualTo(1));
        Assert.That(deck.Field[1, 0], Is.EqualTo(1));
        Assert.That(building.Rotation, Is.Zero);
        Assert.That(deck.Buildings[1], Is.SameAs(building));
        Assert.That(supply.Available, Is.Empty);
    }

    [Test]
    public void RemovalPermissionDefaultsToTrueAndBlocksRemovalWithoutChangingBoard()
    {
        MedievalEra.Server.Core.Game.Interfaces.IPlayerDeck deck = Deck();
        Assert.That(deck.AllowToRemoveBuilding, Is.True);
        var building = Create("xx");
        var supply = new BuildingSupply([building]);
        Assert.That(deck.BuildFromSupply(supply, building, 0, 0, 0), Is.True);
        deck.AllowToRemoveBuilding = false;
        Assert.That(deck.RemoveBuilding(1), Is.False);
        Assert.That(deck.Field[0, 0], Is.EqualTo(1));
        Assert.That(deck.Field[1, 0], Is.EqualTo(1));
        Assert.That(deck.Buildings[1], Is.SameAs(building));
        Assert.That(deck.UnbuiltBuildings, Is.Empty);
        deck.AllowToRemoveBuilding = true;
        Assert.That(deck.RemoveBuilding(1), Is.True);
        Assert.That(deck.ReturnBuilding(supply, building), Is.True);
        Assert.That(deck.Field[0, 0], Is.Null);
        Assert.That(deck.Field[1, 0], Is.Null);
        Assert.That(supply.Available, Does.Contain(building));
    }

    [Test]
    public void BuildFromSupplyRollsBackRejectedPlacementAndUsesSelectedRotation()
    {
        var deck = Deck();
        var building = Create("xx");
        var supply = new BuildingSupply([building]);
        Assert.That(deck.BuildFromSupply(supply, building, 6, 6, 90), Is.False);
        Assert.That(supply.Available, Does.Contain(building));
        Assert.That(deck.UnbuiltBuildings, Is.Empty);
        Assert.That(building.Rotation, Is.Zero);
        Assert.That(deck.BuildFromSupply(supply, building, 13, 13, 90), Is.False);
        Assert.That(deck.BuildFromSupply(supply, building, 0, 0, 45), Is.False);
        Assert.That(deck.BuildFromSupply(supply, building, 13, 0, 90), Is.True);
        Assert.That(deck.Field[13, 0], Is.EqualTo(1));
        Assert.That(deck.Field[13, 1], Is.EqualTo(1));
        Assert.That(supply.Available, Is.Empty);
        Assert.That(deck.BuildFromSupply(supply, building, 0, 0, 0), Is.False);
    }

    [Test]
    public void SupplyTracksIndividualShapesExhaustionAndReturns()
    {
        var first = Create("xx");
        var second = Create("x\nx");
        var supply = new BuildingSupply([first, second]);
        var deck = Deck();
        Assert.That(deck.TakeBuilding(supply, second), Is.True);
        Assert.That(deck.TakeBuilding(supply, second), Is.False);
        Assert.That(supply.Take(Buildings.Wall), Is.SameAs(first));
        Assert.That(supply.CanTake(Buildings.Wall), Is.False);
        Assert.That(supply.Take(Buildings.Wall), Is.Null);
        Assert.That(deck.ReturnBuilding(supply, second), Is.True);
        Assert.That(deck.ReturnBuilding(supply, second), Is.False);
        Assert.That(supply.Available, Does.Contain(second));
    }
}
