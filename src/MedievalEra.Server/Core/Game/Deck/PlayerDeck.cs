using MedievalEra.Server.Core.Game.Dice;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Deck
{
    public class PlayerDeck
    {
        public const int TotalRolls = 3;
        private readonly DiceFactory _diceFactory;
        private readonly List<IDice> _rollingDices = new(4);
        private readonly List<IDice> _freezedDices = new(4);
        public PlayerDeck(string playerName, DiceFactory diceFactory)
        {
            PlayerName = playerName;
            _diceFactory = diceFactory;
            _rollingDices.AddRange(_diceFactory.GetStarterKit());
        }

        /// <summary>
        /// Восстановление из хранилища
        /// </summary>
        /// <param name="data"></param>
        public PlayerDeck(object data)
        {

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
        public int RestRolls { get; } = TotalRolls;
        public IEnumerable<IDice> Dices => RollingDices.Union(FreezedDices);
        public IEnumerable<IDice> RollingDices => _rollingDices;
        public IEnumerable<IDice> FreezedDices => _freezedDices;

        public void Roll()
        {

        }

        public void Finish()
        {

        }
    }
}
