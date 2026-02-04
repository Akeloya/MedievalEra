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

        public bool Equals(IDiceFace? other)
        {
            if(other == null)
                return false;

            if (this == other)
                return true;

            if(DiceType != other.DiceType)
                return false;

            if (Values.Count != other.Values.Count)
                return false;

            var notExact = false;

            foreach(var t in Values)
            {
                if (other.Values.TryGetValue(t.Key, out var value) && value == t.Value)
                    continue;
                notExact = true;
            }

            return !notExact;
        }

        protected abstract void CheckAllowed(Dictionary<DiceResource, int> values);
    }
}
