namespace MedievalEra.Server.Core.Game.Scores
{
    public enum GameType
    {
        Tabletop,
        Online
    }

    public class GameScore
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public GameType GameType { get; set; } = GameType.Tabletop;
        public List<PlayerScore> Players { get; set; } = [];
    }
}
