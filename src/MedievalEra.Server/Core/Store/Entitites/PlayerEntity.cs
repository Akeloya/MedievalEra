namespace MedievalEra.Server.Core.Store.Entitites
{
    public class PlayerEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastGame { get; set; }
    }
}
