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
        public int Score { get; }

        /// <summary>
        /// Потребление ресурсов
        /// </summary>
        /// <param name="deck"></param>
        void ResourceCounsumption(IPlayerDeck deck);
    }
}
