using MedievalEra.Server.Core.Game.Enums;

namespace MedievalEra.Server.Core.Game.Interfaces
{
    public interface IResource
    {
        GamingSource Resource { get; }
        int Count { get; }
    }
}
