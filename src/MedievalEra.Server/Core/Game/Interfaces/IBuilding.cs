using MedievalEra.Server.Core.Game.Enums;

namespace MedievalEra.Server.Core.Game.Interfaces
{
    /// <summary>
    /// Здание, которое строим
    /// </summary>
    public interface IBuilding
    {
        /// <summary>
        /// Сколько очков даёт
        /// </summary>
        public int? Score { get; }
        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; }

        public Buildings Type { get; }

        /// <summary>Количество зданий этого типа в общем игровом наборе.</summary>
        public int Quantity { get; }

        /// <summary>
        /// Потребление ресурсов
        /// </summary>
        /// <param name="deck"></param>
        void ResourceCounsumption(IPlayerDeck deck);
    }
}
