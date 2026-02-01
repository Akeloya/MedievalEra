using MedievalEra.Server.Core.Game.Enums;

namespace MedievalEra.Server.Core.Game.Interfaces
{
    public interface IDiceFace
    {
        DiceType DiceType { get;}
        Dictionary<DiceResouce, int> Values { get; }
    }
}
