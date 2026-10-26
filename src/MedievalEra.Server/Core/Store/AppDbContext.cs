using MedievalEra.Server.Core.Settings;
using MedievalEra.Server.Core.Store.Entitites;
using GameScoreRecord = MedievalEra.Server.Core.Game.Scores.GameScore;
using PlayerScoreRecord = MedievalEra.Server.Core.Game.Scores.PlayerScore;
using BuildingScoreRecord = MedievalEra.Server.Core.Game.Scores.BuildingScore;

using Microsoft.EntityFrameworkCore;

namespace MedievalEra.Server.Core.Store
{
    public class AppDbContext : DbContext
    {
        public DbSet<PlayerEntity> Players { get; set; }
        public DbSet<GameScore> Scores { get; set; }
        public DbSet<GameScoreRecord> GameScores { get; set; }
        public DbSet<PlayerScoreRecord> PlayerScores { get; set; }
        public DbSet<BuildingScoreRecord> BuildingScores { get; set; }

        private readonly DatabaseType _databaseType;
        private readonly string? _connectionString;

        public enum DatabaseType
        {
            Sqlite,
            InMemory
        }

        public AppDbContext(DatabaseSettings settings) : this(settings.Type, settings.ConnectionString)
        {

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
                        var sqliteConnectionString = _connectionString ?? "Data Source=EraMedieval.db";
                        optionsBuilder.UseSqlite(sqliteConnectionString,
                            options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                        break;

                    case DatabaseType.InMemory:
                        var inMemoryName = _connectionString ?? "EraMedieval";
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
                // Ограничения и валидация
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("NOCASE");
                entity.Property(e => e.PasswordHash);
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("IX_Players_Name_NoCase");

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

            modelBuilder.Entity<GameScore>(entity =>
            {
                entity.HasKey(game => game.Id);
                entity.OwnsMany(game => game.PlayerScores, players =>
                {
                    players.WithOwner().HasForeignKey("GameScoreId");
                    players.Property<int>("PlayerScoreId").ValueGeneratedOnAdd();
                    players.HasKey("GameScoreId", "PlayerScoreId");
                    players.OwnsMany(player => player.BuildingScores, buildings =>
                    {
                        buildings.WithOwner().HasForeignKey("GameScoreId", "PlayerScoreId");
                        buildings.Property<int>("BuildingScoreId").ValueGeneratedOnAdd();
                        buildings.HasKey("GameScoreId", "PlayerScoreId", "BuildingScoreId");
                    });
                });
            });

            modelBuilder.Entity<GameScoreRecord>()
                .HasMany(game => game.Players)
                .WithOne(player => player.GameScore)
                .HasForeignKey(player => player.GameScoreId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameScoreRecord>()
                .Property(game => game.GameType)
                .HasConversion<int>();

            modelBuilder.Entity<PlayerScoreRecord>()
                .HasOne(player => player.PlayerEntity)
                .WithMany()
                .HasForeignKey(player => player.PlayerEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlayerScoreRecord>()
                .HasMany(player => player.Buildings)
                .WithOne(building => building.PlayerScore)
                .HasForeignKey(building => building.PlayerScoreId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
