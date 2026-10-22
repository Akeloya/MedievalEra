namespace MedievalEra.Server.Core.Game.Scores
{
    public class GameScore
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<PlayerScore> Players { get; set; } = [];
    }
}
