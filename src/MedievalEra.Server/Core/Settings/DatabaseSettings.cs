using MedievalEra.Server.Core.Store;

namespace MedievalEra.Server.Core.Settings
{
    public class DatabaseSettings
    {
        public AppDbContext.DatabaseType Type { get; set; }
        public string? ConnectionString { get; set; }
    }
}
