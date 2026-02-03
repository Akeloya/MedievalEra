using MedievalEra.Server.Core.Game.Enums;

namespace MedievalEra.Server.Core.Game.Dice.Faces
{
    public class PeasantFace(Dictionary<DiceResource, int> values) : ADiceFace(values)
    {
        public override DiceType DiceType => DiceType.Peasant;
        protected override void CheckAllowed(Dictionary<DiceResource, int> values)
        {
            if (values.Any(p => p.Key is DiceResource.Wood or DiceResource.Stone or DiceResource.Meal
                    or DiceResource.Building or DiceResource.Skull))
                return;
            throw new ArgumentOutOfRangeException(nameof(values));
        }
    }
}
