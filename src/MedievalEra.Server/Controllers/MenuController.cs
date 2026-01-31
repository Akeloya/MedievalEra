using MedievalEra.Server.Core.UI;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MedievalEra.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MenuController : ControllerBase
    {
        [HttpGet(Name ="GetMenu")]
        public IActionResult Get()
        {
            return Ok(new { message = "MenuController работает!", timestamp = DateTime.Now });
        }

        [HttpGet("items")]
        public IActionResult GetMenuItems()
        {
            var menuItems = new List<MenuItem>
        {
            new MenuItem {
                Id = "1",
                Title = "Профиль",
                Icon = "👤",
                Description = "Управление профилем",
                ActionType = "profile"
            },
            new MenuItem {
                Id = "2",
                Title = "Настройки",
                Icon = "⚙️",
                Description = "Настройки системы",
                ActionType = "settings"
            },
            new MenuItem {
                Id = "3",
                Title = "Отчеты",
                Icon = "📊",
                Description = "Просмотр отчетов",
                ActionType = "reports"
            },
            new MenuItem {
                Id = "4",
                Title = "Помощь",
                Icon = "❓",
                Description = "Справка и поддержка",
                ActionType = "help"
            },
            new MenuItem {
                Id = "5",
                Title = "Уведомления",
                Icon = "🔔",
                Description = "Системные уведомления",
                ActionType = "notifications"
            },
            new MenuItem {
                Id = "6",
                Title = "Выход",
                Icon = "🚪",
                Description = "Завершение работы",
                ActionType = "logout"
            }
        };

            return Ok(menuItems);
        }
    }
}
