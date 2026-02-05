using MedievalEra.Server.Core.Game.Dice;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MedievalEra.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DiceController : ControllerBase
    {
        private readonly DiceFactory _diceFactory;
        public DiceController(DiceFactory diceFactory) 
        {
            _diceFactory = diceFactory;
        }

        [HttpGet]
        public IEnumerable<IDice> GetStarterKit()
        {
            return _diceFactory.GetStarterKit();
        }

        [HttpGet("get")]
        public IDice GetDice(/*IPlayer player, */DiceType type)
        {
            return _diceFactory.GetDice(type);
        }
    }
}
