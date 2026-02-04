using MedievalEra.Server.Core.Game.Dice.Faces;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Dice
{
    public sealed class CitizenDice(IRandomProvider randomProvider) : ADice("Горожанин", "LightBlue", DiceType.Citizen, randomProvider)
    {
        public override IReadOnlyCollection<IDiceFace> Faces { get; } =
        [
            new CitizenFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Goods, 2}
            }),
            new CitizenFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Goods, 1}
            }),
            new CitizenFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Stone, 2}
            }),
            new CitizenFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Culture, 1}
            }),
            new CitizenFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Building, 2}
            }),
            new CitizenFace(new Dictionary<DiceResource, int>(){
                { DiceResource.Stone, 1},
                { DiceResource.Skull, 1},
                { DiceResource.Building, 1}
            })
        ];
    }
}
