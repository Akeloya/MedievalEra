using MedievalEra.Server.Core.Game.Interfaces;
using MedievalEra.Server.Core.Store;

namespace MedievalEra.Server.Core.Game.Player
{
    public class PlayerFactory
    {
        private AppDbContext _dbContext;
        public PlayerFactory(AppDbContext dbContext) 
        {
            _dbContext = dbContext;
        }
        public async Task<IPlayer?> GetOrDefault(string name)
        {
            var entity = await _dbContext.Players.FindAsync(name);
            if (entity == null)
                return null;
            return new Player(entity.Id, entity.Name);
        }
    }
}
