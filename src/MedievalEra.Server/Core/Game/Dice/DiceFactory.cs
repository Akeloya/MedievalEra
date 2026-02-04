using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Dice
{
    public class DiceFactory(IRandomProvider randomProvider)
    {
        private readonly IRandomProvider _randomProvider = randomProvider;


        public IDice GetDice(DiceType type)
        {
            return type switch
            {
                DiceType.Citizen => new CitizenDice(_randomProvider),
                DiceType.Notility => new NobilityDice(_randomProvider),
                DiceType.Clergy => new ClergyDice(_randomProvider),
                DiceType.Peasant => new PeasantDice(_randomProvider),
                DiceType.None => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        }

        public IEnumerable<IDice> GetStarterKit()
        {
            yield return GetDice(DiceType.Notility);
            yield return GetDice(DiceType.Peasant);
            yield return GetDice(DiceType.Peasant);
            yield return GetDice(DiceType.Peasant);
        }
    }
}
