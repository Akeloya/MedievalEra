using MedievalEra.Server.Core.Game.Enums;

namespace MedievalEra.Server.Core.Game.Dice.Faces
{
    public class NobilityFace(Dictionary<DiceResource, int> values) : ADiceFace(values)
    {
        public override DiceType DiceType => DiceType.Notility;
        protected override void CheckAllowed(Dictionary<DiceResource, int> values)
        {
            if (values.All(p => p.Key is DiceResource.Attack or DiceResource.Defence or DiceResource.Skull or DiceResource.Goods))
                return;
            throw new ArgumentOutOfRangeException(nameof(values));
        }
    }
}
