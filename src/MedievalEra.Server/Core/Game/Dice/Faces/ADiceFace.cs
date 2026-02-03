using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Dice.Faces
{
    public abstract class ADiceFace : IDiceFace
    {
        protected ADiceFace(Dictionary<DiceResource, int> values)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            CheckAllowed(values);
            Values = values;
        }
        public abstract DiceType DiceType { get; }
        public Dictionary<DiceResource, int> Values { get; }

        protected abstract void CheckAllowed(Dictionary<DiceResource, int> values);
    }
}
