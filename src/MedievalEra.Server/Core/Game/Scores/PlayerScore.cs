namespace MedievalEra.Server.Core.Game.Scores
{
    public class PlayerScore
    {
        public int Id { get; set; }
        public int PlayerNumber { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public List<BuildingScore> Buildings { get; set; } = [];
        public int MarketBonus { get; set; }
        public int GuildResources { get; set; }
        public int UniversityCultureLevel { get; set; }
        public int CathedralDiceCount { get; set; }
        public int CultureIcons { get; set; }
        public int WalledAreaSize { get; set; }
        public int MostDevelopedCultureBonus { get; set; }
        public int LargestWalledAreaBonus { get; set; }
        public int DisasterPoints { get; set; }
        public int IntermediateTotal { get; set; }
        public int FinalTotal { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TitleComment { get; set; } = string.Empty;
        public bool HasTitle { get; set; }

        public int GameScoreId { get; set; }
        public GameScore GameScore { get; set; } = null!;
    }
}
