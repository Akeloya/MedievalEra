using MedievalEra.Server.Core.Game;
using MedievalEra.Server.Core.Game.Common;
using MedievalEra.Server.Core.Game.Deck;
using MedievalEra.Server.Core.Game.Dice;
using MedievalEra.Server.Core.Game.Enums;

namespace MedievalEra.Server.Tests.Core.Game;

[TestFixture]
public class BuildingClusterTests
{
    private static PlayerDeck Deck() => new("Player", new DiceFactory(new DefaultRandomProvider()));

    private static void Place(PlayerDeck deck, int x, int y,
        Buildings type = Buildings.House, string mask = "x", bool rotate = false)
    {
        var building = new Building(type, type.ToString(), null, 1, DiceType.None, mask);
        if (rotate) building.Rotate();
        var supply = new BuildingSupply([building]);
        Assert.That(deck.TakeBuilding(supply, building), Is.True);
        Assert.That(deck.Build(x, y, building), Is.True);
    }

    [Test]
    public void EmptyBoardAndIsolatedMulticellBuildingDoNotCount()
    {
        var deck = Deck();
        Assert.That(deck.CalculateBuildingClaster(), Is.Zero);
        Place(deck, 0, 0, mask: "xx\nxx");
        Place(deck, 2, 2);
        Assert.That(deck.CalculateBuildingClaster(), Is.Zero);
    }

    [Test]
    public void SeparateClustersReturnMaximumAndCountEachInstanceOnce()
    {
        var deck = Deck();
        Place(deck, 0, 0, mask: "xx\nxx");
        Place(deck, 2, 0, mask: "x\nx");
        Place(deck, 0, 2);
        Place(deck, 12, 13);
        Place(deck, 13, 13);
        Assert.That(deck.CalculateBuildingClaster(), Is.EqualTo(3));
        Assert.That(deck.CalculateBuildingClaster(), Is.EqualTo(3));
    }

    [Test]
    public void MaximumUsesDisasterCountRatherThanFirstClusterOrLargestPhysicalGroup()
    {
        var deck = Deck();
        Place(deck, 0, 0, Buildings.Hospital);
        Place(deck, 1, 0);
        Place(deck, 2, 0);
        Place(deck, 3, 0);
        Place(deck, 4, 0); // Пять зданий, из них считаются три.
        for (var x = 0; x < 4; x++) Place(deck, x, 3);
        Place(deck, 0, 6);
        Place(deck, 1, 6);
        Assert.That(deck.CalculateBuildingClaster(), Is.EqualTo(4));
    }

    [TestCase(Buildings.Wall)]
    [TestCase(Buildings.Tower)]
    public void WallsAndTowersDoNotCountOrConnectBuildings(Buildings separator)
    {
        var deck = Deck();
        Place(deck, 0, 0);
        Place(deck, 1, 0, separator);
        Place(deck, 2, 0);
        // По обе стороны стартовой башни также нет кластера.
        Place(deck, 6, 7);
        Place(deck, 8, 7);
        Assert.That(deck.CalculateBuildingClaster(), Is.Zero);
    }

    [Test]
    public void HospitalProtectsOnlyDirectNeighborsInSameCluster()
    {
        var deck = Deck();
        Place(deck, 0, 0, Buildings.Hospital);
        Place(deck, 1, 0);
        Place(deck, 2, 0);
        Place(deck, 3, 0);
        Assert.That(deck.CalculateBuildingClaster(), Is.EqualTo(2));
        Assert.That(deck.RemoveBuilding(1), Is.True);
        Assert.That(deck.CalculateBuildingClaster(), Is.EqualTo(3));
    }

    [Test]
    public void MultipleHospitalsAndDiagonalHospitalApplyOnlyDirectProtection()
    {
        var deck = Deck();
        Place(deck, 0, 0, Buildings.Hospital);
        Place(deck, 1, 0);
        Place(deck, 2, 0, Buildings.Hospital);
        Place(deck, 3, 0);
        Place(deck, 4, 0);
        Place(deck, 5, 1, Buildings.Hospital);
        Assert.That(deck.CalculateBuildingClaster(), Is.EqualTo(1));
    }

    [Test]
    public void RotatedConcaveMaskUsesOccupiedCellsIncludingInnerSides()
    {
        var deck = Deck();
        Place(deck, 0, 0, Buildings.Hospital, "xxx\nxox\nxox", rotate: true);
        Place(deck, 0, 1); // Внутри повёрнутой П-образной маски.
        Place(deck, 4, 0); // Отдельная группа за пределами госпиталя.
        Place(deck, 5, 0);
        Assert.That(deck.CalculateBuildingClaster(), Is.EqualTo(2));
    }
}
