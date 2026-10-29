namespace MedievalEra.Server.Core.Game.Interfaces
{
    public interface IPlayerDeck
    {
        public IPlayer Player { get; }
        public IResource Goods { get; }
        public IResource Stone { get; }
        public IResource Wood { get; }
        public IResource Meal { get; }

        public IReadOnlyCollection<IResource> GameResources { get; }
        public IResource Skull { get; }
        public IResource Culture { get; }
    }
}
