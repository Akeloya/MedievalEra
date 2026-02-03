using MedievalEra.Server.Core.Game.Enums;

namespace MedievalEra.Server.Core.Game.Deck
{
    public class PlayerDeck
    {
        public PlayerDeck(string playerName)
        {
            PlayerName = playerName;
        }
        public string PlayerName { get; }
        public Dictionary<GamingSource, int> Resources { get; } = new()
        {
            { GamingSource.Goods, 0},
            { GamingSource.Stone, 1},
            { GamingSource.Wood, 2},
            { GamingSource.Meal, 3}
        };
        public int Skulls { get; }
        public int Culture { get; }
    }
}
