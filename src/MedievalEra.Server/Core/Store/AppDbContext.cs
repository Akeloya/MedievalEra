using MedievalEra.Server.Core.Game.Player;
using MedievalEra.Server.Core.Store.Entitites;

using Microsoft.EntityFrameworkCore;

namespace MedievalEra.Server.Core.Store
{
    public class AppDbContext : DbContext
    {
        public DbSet<PlayerEntity> Players { get; set; }

        private readonly DatabaseType _databaseType;
        private readonly string? _connectionString;

        public enum DatabaseType
        {
            Sqlite,
            InMemory
        }

        public AppDbContext(DatabaseType databaseType = DatabaseType.Sqlite,
                           string? connectionString = null)
        {
            _databaseType = databaseType;
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                switch (_databaseType)
                {
                    case DatabaseType.Sqlite:
                        var sqliteConnectionString = _connectionString ?? "Data Source=players.db";
                        optionsBuilder.UseSqlite(sqliteConnectionString,
                            options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                        break;

                    case DatabaseType.InMemory:
                        var inMemoryName = _connectionString ?? "TestDatabase";
                        optionsBuilder.UseInMemoryDatabase(inMemoryName);
                        break;

                    default:
                        throw new ArgumentException($"Unsupported database type: {_databaseType}");
                }
            }

            // Включаем детализацию ошибок (только для разработки)
#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PlayerEntity>(entity =>
            {
                // Первичный ключ
                entity.HasKey(e => e.Id);

                // Индексы для оптимизации запросов
                entity.HasIndex(e => e.Name).IsUnique();

                // Ограничения и валидация
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                // Для SQLite специфичные настройки
                if (Database.IsSqlite())
                {
                    entity.Property(e => e.CreatedAt)
                        .HasConversion(
                            v => v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

                    entity.Property(e => e.LastGame)
                        .HasConversion(
                            v => v.HasValue ? v.Value.ToUniversalTime() : (DateTime?)null,
                            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
                }
            });
        }
    }
}
