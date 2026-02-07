using MedievalEra.Server.Core.Game.Enums;

namespace MedievalEra.Server.Core.Game.Interfaces
{
    public interface IDiceFace : IEquatable<IDiceFace>
    {
        DiceType DiceType { get;}
        Dictionary<DiceResource, int> Values { get; }
        /// <summary>
        /// Если нужно выбрать из предложенной коллекции ресурсов или действий
        /// </summary>
        bool Choose { get; }
    }
}
