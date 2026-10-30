using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game
{
    public class BuildingsFactory
    {
        public static IReadOnlyList<IBuilding> GameSet { get; } = Array.AsReadOnly<IBuilding>(
        [
            new Building(Buildings.ScorchedEarth, "Выжженая земля", null, 27, DiceType.None, """
                xxx
                xxx
                xxx
                """),
            new Building(Buildings.Wall, "Стена", null, 3, DiceType.None,"""xx"""),
            new Building(Buildings.Wall, "Стена", null, 4, DiceType.None,"""xxx"""),
            new Building(Buildings.Wall, "Стена", null, 18, DiceType.None,"""xxxx"""),
            new Building(Buildings.Wall, "Стена", null, 3, DiceType.None,"""xxxxxx"""),
            new Building(Buildings.Tower, "Башня", 1, 8, DiceType.Nobility,"""
                xx
                xx
                """),
            new Building(Buildings.House, "Дом", 1, 18, DiceType.Peasant, """xx"""),
            new Building(Buildings.Farm, "Ферма", 1, 16, DiceType.None,"""
                xxx
                xxx
                """),
            new Building(Buildings.Church, "Церковь", 1, 5, DiceType.Clergy,"""xxx"""),
            new Building(Buildings.Monastery, "Монастырь", 4, 3, DiceType.None,"""
                xxo
                xxx
                """),
            new Building(Buildings.Cathedral, "Собор", 5, 2, DiceType.None,"""
                oxoo
                xxxx
                oxoo
                """),
            new Building(Buildings.CityHouse, "Городской дом", 1, 5, DiceType.Citizen,"""
                xx
                ox
                """),
            new Building(Buildings.Guild, "Гильдия", 3, 3, DiceType.None,"""
                xox
                xxx
                """),
            new Building(Buildings.Market, "Рынок", 2, 3, DiceType.None,"""
                xx
                xx
                """),
            new Building(Buildings.Hospital, "Госпиталь", 3, 3, DiceType.None,"""
                oxo
                xxx
                oxo
                """),
            new Building(Buildings.Sawmill, "Лесопилка", 2, 3, DiceType.None,"""
                xxo
                oxx
                """),
            new Building(Buildings.University, "Университет", 4, 2, DiceType.None,"""
                xox
                xox
                xxx
                """)
        ]);

        public IBuilding Get(Buildings type)
        {
            var definition = GameSet.FirstOrDefault(building => building.Type == type)
                ?? throw new ArgumentOutOfRangeException(nameof(type), type, "Тип отсутствует в игровом наборе.");
            return new Building(definition.Type, definition.Name, definition.Score,
                definition.Quantity, definition.DiceType, definition.Masks[0].ToString());
        }

        public bool CanBuild(Buildings type)
        {
            return GameSet.Any(building => building.Type == type && building.Quantity > 0);
        }
    }
}
