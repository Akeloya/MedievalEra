using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Deck
{
    public class GameResource : IResource
    {
        public GameResource(GamingSource resource, int count, int maxValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxValue);
            if (count < 0 || count > maxValue)
                throw new ArgumentOutOfRangeException(nameof(count));

            Resource = resource;
            Count = count;
            MaxValue = maxValue;
        }

        public GamingSource Resource { get; }
        public int Count { get; private set; }
        public int MaxValue { get; }

        public bool TryAdd(int count)
        {
            if (count < 0 || count > MaxValue - Count) return false;
            Count += count;
            return true;
        }

        public bool TryDecrase(int count)
        {
            if (count < 0 || count > Count) return false;
            Count -= count;
            return true;
        }
    }
}
