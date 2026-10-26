using MedievalEra.Server.Core.Game.Scores;

namespace MedievalEra.Server.Tests.Core.Game.Scores
{
    [TestFixture]
    public class GameScoreTitleTests
    {
        [TestCase(44, "Раб", "Терпимо. Продолжайте возделывать землю.")]
        [TestCase(45, "Крестьянин", "Неплохое начало. Вы хотя бы свободны.")]
        [TestCase(64, "Крестьянин", "Неплохое начало. Вы хотя бы свободны.")]
        [TestCase(65, "Торговец", "Хорошее предложение. Повысим ставки?")]
        [TestCase(79, "Торговец", "Хорошее предложение. Повысим ставки?")]
        [TestCase(80, "Рыцарь", "Отличный турнир. Сумеете лучше в следующий раз?")]
        [TestCase(89, "Рыцарь", "Отличный турнир. Сумеете лучше в следующий раз?")]
        [TestCase(90, "Знать", "Величественное представление!")]
        [TestCase(94, "Знать", "Величественное представление!")]
        [TestCase(95, "Монарх", "Королевское достижение!")]
        [TestCase(99, "Монарх", "Королевское достижение!")]
        [TestCase(100, "Папа", "Ваши кубики, несомненно, были благословлены!")]
        public void ForScore_ReturnsExpectedTitleAndComment(int score, string expectedTitle, string expectedComment)
        {
            var result = GameScoreTitle.ForScore(score);

            Assert.That(result.Title, Is.EqualTo(expectedTitle));
            Assert.That(result.Comment, Is.EqualTo(expectedComment));
        }
    }
}
