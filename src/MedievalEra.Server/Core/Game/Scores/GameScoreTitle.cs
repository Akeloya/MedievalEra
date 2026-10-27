namespace MedievalEra.Server.Core.Game.Scores
{
    public static class GameScoreTitle
    {
        public static IReadOnlyList<GameScoreTitleRange> Ranges { get; } = Array.AsReadOnly<GameScoreTitleRange>(
        [
            new(44, "Раб", "Терпимо. Продолжайте возделывать землю."),
            new(64, "Крестьянин", "Неплохое начало. Вы хотя бы свободны."),
            new(79, "Торговец", "Хорошее предложение. Повысим ставки?"),
            new(89, "Рыцарь", "Отличный турнир. Сумеете лучше в следующий раз?"),
            new(94, "Знать", "Величественное представление!"),
            new(99, "Монарх", "Королевское достижение!"),
            new(null, "Папа", "Ваши кубики, несомненно, были благословлены!")
        ]);

        public static (string Title, string Comment) ForScore(int score)
        {
            var range = Ranges.First(range => range.MaxScore is null || score <= range.MaxScore.Value);
            return (range.Title, range.Comment);
        }
    }

    public sealed record GameScoreTitleRange(int? MaxScore, string Title, string Comment);
}
