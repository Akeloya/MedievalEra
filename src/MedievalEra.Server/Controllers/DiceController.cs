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
        ILogger<DiceController> _logger;
        public DiceController(DiceFactory diceFactory, ILoggerFactory loggerFactory) 
        {
            _diceFactory = diceFactory;
            _logger = loggerFactory.CreateLogger<DiceController>();
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogTrace("Entered into get");
            return Ok(new { message = "работает!", timestamp = DateTime.Now });
        }

        [HttpGet("StarterKit")]
        public IEnumerable<IDice> GetStarterKit()
        {
            _logger.LogTrace("Entered into GetStarterKit");
            return _diceFactory.GetStarterKit();
        }

        [HttpGet("get")]
        public IDice GetDice(/*IPlayer player, */DiceType type)
        {
            _logger.LogTrace("Entered into GetDice");
            return _diceFactory.GetDice(type);
        }
    }
}
