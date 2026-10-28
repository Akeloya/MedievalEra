using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game
{
    public class BuildingsFactory
    {
        public static IReadOnlyList<IBuilding> GameSet { get; } = Array.AsReadOnly<IBuilding>(
        [
            new Building(Buildings.ScorchedEarth, "Выжженая земля", null, 27, DiceType.None),
            new Building(Buildings.Wall, "Стена", null, 32, DiceType.None),
            new Building(Buildings.Tower, "Башня", 1, 8, DiceType.Nobility),
            new Building(Buildings.House, "Дом", 1, 10, DiceType.Peasant),
            new Building(Buildings.Farm, "Ферма", 1, 16, DiceType.None),
            new Building(Buildings.Church, "Церковь", 1, 5, DiceType.Clergy),
            new Building(Buildings.Monastery, "Монастырь", 4, 3, DiceType.None),
            new Building(Buildings.Cathedral, "Собор", 5, 2, DiceType.None),
            new Building(Buildings.CityHouse, "Городской дом", 1, 5, DiceType.Citizen),
            new Building(Buildings.Guild, "Гильдия", 3, 3, DiceType.None),
            new Building(Buildings.Market, "Рынок", 2, 3, DiceType.None),
            new Building(Buildings.Hospital, "Больница", 3, 3, DiceType.None),
            new Building(Buildings.Sawmill, "Лесопилка", 2, 3, DiceType.None),
            new Building(Buildings.University, "Университет", 4, 2, DiceType.None)
        ]);

        public IBuilding Get(Buildings type)
        {
            return GameSet.FirstOrDefault(building => building.Type == type)
                ?? throw new ArgumentOutOfRangeException(nameof(type), type, "Тип отсутствует в игровом наборе.");
        }

        public bool CanBuild(Buildings type)
        {
            return GameSet.Any(building => building.Type == type && building.Quantity > 0);
        }
    }
}
