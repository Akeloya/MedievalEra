using MedievalEra.Server.Core.Game.Enums;

namespace MedievalEra.Server.Core.Game.Scores
{
    public class BuildingScore
    {
        public int Id { get; set; }
        public Buildings Building { get; set; }
        public int Count { get; set; }
        public int InsideWallCount { get; set; }
        public int Points { get; set; }

        public int PlayerScoreId { get; set; }
        public PlayerScore PlayerScore { get; set; } = null!;
    }
}
