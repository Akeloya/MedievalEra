using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game
{
    public sealed record Building(Buildings Type, string Name, int? Score, int Quantity) : IBuilding
    {
        public void ResourceCounsumption(IPlayerDeck deck)
        {
            ArgumentNullException.ThrowIfNull(deck);
            // IPlayerDeck пока не предоставляет ресурсов для списания.
        }
    }
}
