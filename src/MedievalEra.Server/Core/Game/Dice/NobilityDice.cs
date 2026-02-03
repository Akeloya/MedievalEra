using MedievalEra.Server.Core.Game.Dice.Faces;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Dice
{
    public class NobilityDice : IDice
    {
        public string Name => "Дворянство";
        public string Color => "Gray";
        public DiceType DiceType => DiceType.Notility;
        public IReadOnlyCollection<IDiceFace> Faces { get; } = new List<IDiceFace>
        {
            new NobilityFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Attack, 3}
            }),
            new NobilityFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Attack, 2}
            }),
            new NobilityFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Attack, 1}
            }),
            new NobilityFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Defence, 4}
            }),
            new NobilityFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Goods, 1}
            }),
            new NobilityFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Goods, 2},
                { DiceResource.Skull, 1}
            })
        };
    }
}
