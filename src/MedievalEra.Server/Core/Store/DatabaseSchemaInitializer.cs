using Microsoft.EntityFrameworkCore;

namespace MedievalEra.Server.Core.Store
{
    public static class DatabaseSchemaInitializer
    {
        public static async Task EnsureCurrentSchemaAsync(AppDbContext dbContext)
        {
            if (!dbContext.Database.IsSqlite())
            {
                return;
            }

            await dbContext.Database.OpenConnectionAsync();
            try
            {
                await EnsureColumnAsync(dbContext, "Players", "PasswordHash");
                await EnsureColumnAsync(dbContext, "GameScores", "GameType");
                await EnsureColumnAsync(dbContext, "PlayerScores", "PlayerEntityId");
                await EnsureColumnAsync(dbContext, "PlayerScores", "ResultAccepted");
                await dbContext.Database.ExecuteSqlRawAsync(
                    "CREATE INDEX IF NOT EXISTS \"IX_Players_Name_NoCase\" ON \"Players\" (\"Name\" COLLATE NOCASE)");
            }
            finally
            {
                await dbContext.Database.CloseConnectionAsync();
            }
        }

        private static async Task EnsureColumnAsync(
            AppDbContext dbContext,
            string tableName,
            string columnName)
        {
            var connection = dbContext.Database.GetDbConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info(\"{tableName}\")";

            var columnExists = false;
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        columnExists = true;
                        break;
                    }
                }
            }

            if (!columnExists)
            {
                var alterTableStatement = (tableName, columnName) switch
                {
                    ("Players", "PasswordHash") =>
                        "ALTER TABLE \"Players\" ADD COLUMN \"PasswordHash\" TEXT NULL",
                    ("GameScores", "GameType") =>
                        "ALTER TABLE \"GameScores\" ADD COLUMN \"GameType\" INTEGER NOT NULL DEFAULT 0",
                    ("PlayerScores", "PlayerEntityId") =>
                        "ALTER TABLE \"PlayerScores\" ADD COLUMN \"PlayerEntityId\" INTEGER NOT NULL DEFAULT 0",
                    ("PlayerScores", "ResultAccepted") =>
                        "ALTER TABLE \"PlayerScores\" ADD COLUMN \"ResultAccepted\" INTEGER NOT NULL DEFAULT 1",
                    _ => throw new InvalidOperationException("Unexpected schema migration column.")
                };
                await dbContext.Database.ExecuteSqlRawAsync(alterTableStatement);
            }
        }
    }
}
