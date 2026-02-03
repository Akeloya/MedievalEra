using MedievalEra.Server.Core.Game.Dice.Faces;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Dice
{
    public class PeasantDice : IDice
    {
        public string Name => "Крестьянин";
        public string Color => "Orange";
        public DiceType DiceType => DiceType.Peasant;
        public IReadOnlyCollection<IDiceFace> Faces { get; } = new List<IDiceFace>()
        {
            new PeasantFace(new Dictionary<DiceResource, int>() {
                { DiceResource.Meal, 3}
            }),
            new PeasantFace(new Dictionary<DiceResource, int>() {
                { DiceResource.Wood, 3}
            }),
            new PeasantFace(new Dictionary<DiceResource, int>() {
                { DiceResource.Stone, 2}
            }),
            new PeasantFace(new Dictionary<DiceResource, int>() {
                { DiceResource.Meal, 2},
                { DiceResource.Stone, 1 }
            }),
            new PeasantFace(new Dictionary<DiceResource, int>() {
                { DiceResource.Building, 2}
            }),
            new PeasantFace(new Dictionary<DiceResource, int>() {
                { DiceResource.Wood, 1},
                { DiceResource.Skull, 1},
                { DiceResource.Building, 1}
            }),
        };
    }
}
