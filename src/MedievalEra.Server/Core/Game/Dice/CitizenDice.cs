using MedievalEra.Server.Core.Game.Dice.Faces;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Dice
{
    public class CitizenDice : IDice
    {
        public string Name => "Горожанин";
        public string Color => "LightBlue";
        public DiceType DiceType => DiceType.Citizen;
        public IReadOnlyCollection<IDiceFace> Faces { get; } = new List<IDiceFace>
        {
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
        };
    }
}
