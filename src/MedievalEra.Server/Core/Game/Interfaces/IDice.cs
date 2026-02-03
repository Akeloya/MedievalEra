using MedievalEra.Server.Core.Game.Enums;

namespace MedievalEra.Server.Core.Game.Interfaces
{
    public interface IDice
    {
        string Name { get; }
        string Color { get; }
        DiceType DiceType { get; }
        IReadOnlyCollection<IDiceFace> Faces { get; }
    }
}
