using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game
{
    public sealed class Building : IBuilding
    {
        public Building(Buildings type, string name, int? score, int quantity, DiceType diceType, string mask = "x")
        {
            Type = type;
            Name = name;
            Score = score;
            Quantity = quantity;
            DiceType = diceType;
            var rotations = new BuildingMask[4];
            rotations[0] = new BuildingMask(mask);
            for (var i = 1; i < rotations.Length; i++)
                rotations[i] = rotations[i - 1].RotateClockwise();
            Masks = Array.AsReadOnly(rotations);
        }

        public Buildings Type { get; }
        public string Name { get; }
        public int? Score { get; }
        public int Quantity { get; }
        public DiceType DiceType { get; }
        public IReadOnlyList<BuildingMask> Masks { get; }
        public int Rotation { get; private set; }
        public BuildingMask Mask => Masks[Rotation / 90];
        public bool IsInsideWall { get; set; }
        public bool Locked { get; set; } = false;
        internal bool IsPlaced { get; set; }
        public void Rotate()
        {
            if (IsPlaced || Locked)
                throw new InvalidOperationException("Перед поворотом снимите здание с доски.");
            Rotation = (Rotation + 90) % 360;
        }

        public void ResourceCounsumption(IPlayerDeck deck)
        {
            ArgumentNullException.ThrowIfNull(deck);
            // IPlayerDeck пока не предоставляет ресурсов для списания.
        }
    }
}
