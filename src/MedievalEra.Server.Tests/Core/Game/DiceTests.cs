using MedievalEra.Server.Core.Game.Common;
using MedievalEra.Server.Core.Game.Dice;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Tests.Core.Game
{
    [TestFixture]
    internal class DiceTests
    {
        private DiceFactory _diceFactory;
        [SetUp]
        public void SetUp()
        {
            _diceFactory = new DiceFactory(new DefaultRandomProvider());
        }

        [Test]
        [TestCaseSource(typeof(DiceTests),nameof(DiceTypeSource))]
        public void GetDiceTest(DiceType type)
        {
            if (type is DiceType.None)
            {
                Assert.Throws<NotImplementedException>(() => _diceFactory.GetDice(type));
                return;
            }

            var dice = _diceFactory.GetDice(type);

            Assert.That(dice.Name, Is.Not.Null);
            Assert.That(dice.DiceType, Is.EqualTo(type));
            Assert.That(dice.Faces, Is.Not.Null);
            Assert.That(dice.Faces.Count, Is.EqualTo(6));
        }

        [Test]
        [TestCaseSource(typeof(DiceTests), nameof(DiceTypeSource))]
        public void RollDiceTest(DiceType type)
        {
            if (type is DiceType.None)
            {
                Assert.Throws<NotImplementedException>(() => _diceFactory.GetDice(type));
                return;
            }
            var dice = _diceFactory.GetDice(type);

            const int RollCount = 100;

            var rollSet = new Dictionary<IDiceFace, int>();
            for(var i = 0; i < RollCount; i++)
            {
                var face = dice.Roll();
                if (!rollSet.TryGetValue(face, out var value))
                {
                    rollSet[face] = 1;
                    continue;
                }
                rollSet[face]++;
            }

            Assert.That(rollSet.Keys.Count, Is.EqualTo(6));

            var min = rollSet.Values.Min();
            var max = (double)rollSet.Values.Max();

            Assert.That(max / min, Is.LessThan(3));
        }

        [Test]
        [TestCaseSource(typeof(DiceTests), nameof(DiceTypeSource))]
        public void RollFixedDiceTest(DiceType type)
        {
            if (type is DiceType.None)
            {
                Assert.Throws<NotImplementedException>(() => _diceFactory.GetDice(type));
                return;
            }
            var dice = _diceFactory.GetDice(type);

            const int RollCount = 100;

            var lockingFace = dice.Faces.First();
            dice.Lock(lockingFace);

            var rollSet = new Dictionary<IDiceFace, int>();
            for (var i = 0; i < RollCount; i++)
            {
                var face = dice.Roll();
                Assert.That(face, Is.EqualTo(lockingFace));
                if (!rollSet.TryGetValue(face, out var value))
                {
                    rollSet[face] = 1;
                    continue;
                }
                rollSet[face]++;
            }

            Assert.That(rollSet.Keys.Count, Is.EqualTo(1));
            Assert.That(rollSet.Values.Sum(), Is.EqualTo(RollCount));
        }

        public static IEnumerable<DiceType> DiceTypeSource { get; } = Enum.GetValues<DiceType>();
    }
}
