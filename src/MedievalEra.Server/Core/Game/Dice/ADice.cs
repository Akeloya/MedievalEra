using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Dice
{
    public abstract class ADice(string name, string color, DiceType diceType, IRandomProvider randomProvider) : IDice
    {
        private readonly IRandomProvider _randomProvider = randomProvider;
        private IDiceFace? _lockedFace;

        public string Name { get; } = name;

        public string Color { get; } = color;

        public DiceType DiceType { get; } = diceType;

        public abstract IReadOnlyCollection<IDiceFace> Faces { get;}

        public void Lock(IDiceFace face)
        {
            if (Faces.All(p => p != face))
                throw new ArgumentOutOfRangeException(nameof(face));

            _lockedFace = face;
        }

        public virtual IDiceFace Roll()
        {
            if (Faces.Count <= 0)
                throw new ApplicationException();

            if (_lockedFace != null)
                return _lockedFace;

            var value = _randomProvider.Next(0, Faces.Count);
            return Faces.ElementAt(value);
        }

        public void Unlock()
        {
            _lockedFace = null;
        }
    }
}
