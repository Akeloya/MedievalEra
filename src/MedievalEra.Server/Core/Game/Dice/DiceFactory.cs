using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Dice
{
    public class DiceFactory
    {
        public IDice GetDice(DiceType type)
        {
            return type switch
            {
                DiceType.Citizen => new CitizenDice(),
                DiceType.Notility => new NobilityDice(),
                DiceType.Clergy => new ClergyDice(),
                DiceType.Peasant => new PeasantDice(),
                DiceType.None => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        } 
    }
}
