namespace MedievalEra.Server.Core.Game.Interfaces
{
    public interface IPlayerDeck
    {
        int?[,] Field { get; }
        IReadOnlyDictionary<int, IBuilding> Buildings { get; }
        IReadOnlyList<IBuilding> UnbuiltBuildings { get; }
        bool AllowToRemoveBuilding { get; set; }
        void CompleteConstruction();
        bool TakeBuilding(BuildingSupply supply, IBuilding building);
        bool ReturnBuilding(BuildingSupply supply, IBuilding building);
        bool CanBuild(int x, int y, IBuilding building);
        bool Build(int x, int y, IBuilding building);
        bool RemoveBuilding(int id);
        bool MoveBuilding(int id, int x, int y, int rotation);
        bool BuildFromSupply(BuildingSupply supply, IBuilding building, int x, int y, int rotation);
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
