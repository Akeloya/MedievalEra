using MedievalEra.Server.Core.Game.Dice.Faces;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Dice
{
    public sealed class PeasantDice(IRandomProvider randomProvider) : ADice("Крестьянин", "Orange", DiceType.Peasant, randomProvider)
    {
        public override IReadOnlyCollection<IDiceFace> Faces { get; } =
        [
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
        ];
    }
}
