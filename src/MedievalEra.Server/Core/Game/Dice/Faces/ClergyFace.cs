using MedievalEra.Server.Core.Game.Enums;

namespace MedievalEra.Server.Core.Game.Dice.Faces
{
    public class ClergyFace(Dictionary<DiceResource, int> values) : ADiceFace(values)
    {
        public override DiceType DiceType => DiceType.Clergy;
        protected override void CheckAllowed(Dictionary<DiceResource, int> values)
        {
            if (values.All(p => p.Key is DiceResource.Culture))
                return;
        }
    }
}
