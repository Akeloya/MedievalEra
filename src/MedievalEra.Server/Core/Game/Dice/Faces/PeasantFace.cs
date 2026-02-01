using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Dice.Faces
{
    public class PeasantFace : IDiceFace
    {
        public PeasantFace(Dictionary<DiceResouce, int> values)
        {
            Values = values;
        }
        public DiceType DiceType => DiceType.Peasant;

        public Dictionary<DiceResouce, int> Values { get; }
    }
}
