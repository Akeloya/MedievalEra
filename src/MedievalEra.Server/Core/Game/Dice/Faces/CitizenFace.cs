using MedievalEra.Server.Core.Game.Enums;

namespace MedievalEra.Server.Core.Game.Dice.Faces
{
    public class CitizenFace(Dictionary<DiceResource, int> values) : ADiceFace(values)
    {
        public override DiceType DiceType => DiceType.Citizen;
        protected override void CheckAllowed(Dictionary<DiceResource, int> values)
        {
            if (values.All(p => p.Key is DiceResource.Culture or DiceResource.Goods or DiceResource.Stone))
                return;
            throw new ArgumentOutOfRangeException(nameof(values));
        }
    }
}
