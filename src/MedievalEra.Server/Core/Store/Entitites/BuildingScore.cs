using MedievalEra.Server.Core.Game.Enums;

namespace MedievalEra.Server.Core.Store.Entitites
{
    public class BuildingScore
    {
        public Buildings Type { get; set; }
        public bool IntoWall { get; set; }
        public int Count { get; set; }
    }
}
