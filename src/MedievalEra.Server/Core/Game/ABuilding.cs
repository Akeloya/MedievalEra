using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game
{
    public abstract class ABuilding : IGameObj
    {
        protected ABuilding(string name, string description)
        {
            Name = name;
            Description = description;
        }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public virtual void Play()
        {
            
        }
    }
}
