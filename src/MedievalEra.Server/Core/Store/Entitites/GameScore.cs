namespace MedievalEra.Server.Core.Store.Entitites
{
    public class PlayerScore
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public int Skulls { get; set; }
        public int Culture { get; set; }
        public int Resources { get; set; }
        public int Dices { get; set; }
        public int MarketBonus { get; set; }
        public int CathedralBonus { get; set; }
        public int GuildBonus { get; set; }
        public int UniversityBonus { get; set; }
        public bool HasMaxWall { get; set; }
        public List<BuildingScore> BuildingScores { get; set; } = [];
    }
    public class GameScore
    {
        public int Id { get; set; }
        public int TurnCount { get; set; }
        public DateTime PlayedTime { get; set; }
        public string? Scenario { get; set; }
        public string Winner { get; set; }
        public string WinnerRank { get; set; }
        public List<PlayerScore> PlayerScores { get; } = [];
    }
}
