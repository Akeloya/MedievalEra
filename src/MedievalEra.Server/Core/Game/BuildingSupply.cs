using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game;

/// <summary>Общий запас одной партии. Каждый элемент — отдельный физический экземпляр.</summary>
public sealed class BuildingSupply
{
    private readonly List<IBuilding> available;
    private readonly HashSet<IBuilding> issued = new(ReferenceEqualityComparer.Instance);
    public IReadOnlyList<IBuilding> Available => available.AsReadOnly();

    public BuildingSupply() : this(BuildingsFactory.GameSet.SelectMany(definition =>
        Enumerable.Range(0, definition.Quantity).Select(_ => (IBuilding)new Building(
            definition.Type, definition.Name, definition.Score, definition.Quantity,
            definition.DiceType, definition.Masks[0].ToString())))) { }

    public BuildingSupply(IEnumerable<IBuilding> buildings)
    {
        available = buildings.ToList();
        if (available.Any(b => b is null) || available.Distinct(ReferenceEqualityComparer.Instance).Count() != available.Count)
            throw new ArgumentException("Запас должен содержать разные экземпляры зданий.", nameof(buildings));
    }

    public bool CanTake(Buildings type) => available.Any(b => b.Type == type);

    internal IReadOnlyList<IBuilding> IssueStartingBuildings(IEnumerable<(Buildings Type, string? Mask)> requests)
    {
        var candidates = available.ToList();
        var selected = new List<IBuilding>();
        foreach (var request in requests)
        {
            var building = candidates.FirstOrDefault(item => item.Type == request.Type &&
                (request.Mask is null || item.Masks[0].ToString() == request.Mask));
            if (building is null)
                throw new InvalidOperationException("В общем запасе недостаточно зданий для стартового набора.");
            candidates.Remove(building);
            selected.Add(building);
        }
        foreach (var building in selected) Take(building);
        return selected;
    }

    public IBuilding? Take(Buildings type)
    {
        var building = available.FirstOrDefault(b => b.Type == type);
        return building is not null && Take(building) ? building : null;
    }

    public bool Take(IBuilding building)
    {
        var index = available.FindIndex(b => ReferenceEquals(b, building));
        if (index < 0) return false;
        available.RemoveAt(index);
        issued.Add(building);
        return true;
    }

    public bool Return(IBuilding building)
    {
        if (building is Building { IsPlaced: true }) return false;
        if (!issued.Remove(building)) return false;
        building.IsInsideWall = false;
        available.Add(building);
        return true;
    }
}
