using MedievalEra.Server.Core.Game.Dice.Faces;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Dice
{
    public class ClergyDice : IDice
    {
        public string Name => "Духовенство";
        public string Color => "White";
        public DiceType DiceType => DiceType.Clergy;
        public IReadOnlyCollection<IDiceFace> Faces { get; } = new List<IDiceFace>
        {
            new ClergyFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Meal, 2}
            }),
            new ClergyFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Stone, 1},
                { DiceResource.Wood, 2}
            }),
            new ClergyFace(new Dictionary<DiceResource, int>(){
                { DiceResource.NewRoll, 1}
            }),
            new ClergyFace(new Dictionary<DiceResource, int>(){
                { DiceResource.NewRoll, 1}
            }),
            new ClergyFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Culture, 1},
            }),
            new ClergyFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Culture, 2},
                { DiceResource.Skull, 1}
            })
        };
    }
}
