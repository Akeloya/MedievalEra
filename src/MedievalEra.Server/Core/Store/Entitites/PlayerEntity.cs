namespace MedievalEra.Server.Core.Store.Entitites
{
    public class PlayerEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastGame { get; set; }
        public string? PasswordHash { get; set; }
    }
}
