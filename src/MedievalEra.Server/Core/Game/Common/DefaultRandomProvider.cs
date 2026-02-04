using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Common
{
    public class DefaultRandomProvider : IRandomProvider
    {
        public int Next(int min, int max)
        {
            return Random.Shared.Next(min, max);
        }
    }
}
