using MedievalEra.Server.Core.Game;
using Microsoft.AspNetCore.Mvc;

namespace MedievalEra.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BuildingsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(BuildingsFactory.GameSet.Where(p=> p.Score.HasValue).Select(building => new
        {
            Id = (int)building.Type,
            Key = building.Type.ToString(),
            Label = building.Name,
            Points = building.Score,
            building.Quantity
        }));
    }
}
