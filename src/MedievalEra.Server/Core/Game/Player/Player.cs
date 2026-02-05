using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Player
{
    public class Player : IPlayer
    {
        public Player(int id, string name) 
        {
            Id = id;
            Name = name;
        }
        public int Id { get; }

        public string Name { get; }
    }
}
