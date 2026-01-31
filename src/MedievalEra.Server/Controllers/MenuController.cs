using MedievalEra.Server.Core.UI;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MedievalEra.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MenuController : ControllerBase
    {
        [HttpGet(Name = "GetMenu")]
        public IActionResult Get()
        {
            return Ok(new { message = "MenuController работает!", timestamp = DateTime.Now });
        }

        [HttpGet("items")]
        public IActionResult GetMenuItems()
        {
            var i = 1;
            var menuItems = new List<MenuItem>
        {
            new MenuItem {
                Id = $"{i++}",
                Title = "Профиль",
                Icon = "👤",
                Description = "Управление профилем",
                ActionType = "profile"
            },
            new MenuItem {
                Id = $"{i++}",
                Title = "Настройки игры",
                Icon = "⚙️",
                Description = "Настройки системы",
                ActionType = "settings"
            },
            new MenuItem {
                Id = $"{i++}",
                Title = "Результаты",
                Icon = "📊",
                Description = "Просмотр результатов игр",
                ActionType = "scores"
            },
            new MenuItem {
                Id = $"{i++}",
                Title = "Помощь",
                Icon = "❓",
                Description = "Справка и поддержка",
                ActionType = "help"
            },
            new MenuItem {
                Id = $"{i++}",
                Title = "Уведомления",
                Icon = "🔔",
                Description = "Системные уведомления",
                ActionType = "notifications"
            },
            new MenuItem {
                Id = $"{i++}",
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
