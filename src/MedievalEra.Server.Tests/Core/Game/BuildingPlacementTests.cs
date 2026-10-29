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
    [TestCase("x\n")]
    public void InvalidMasksAreRejected(string mask) =>
        Assert.Throws<ArgumentException>(() => new BuildingMask(mask));

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
        Assert.That(deck.Build(6, 7, wall), Is.False);
        Assert.That(deck.Field[6, 7], Is.Null);
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
