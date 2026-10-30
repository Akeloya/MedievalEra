using MedievalEra.Server.Core.Game.Dice;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;
using MedievalEra.Server.Core.Game.Player;
using GameSession = MedievalEra.Server.Core.Game.Game;

namespace MedievalEra.Server.Tests.Core.Game;

public class GameTests
{
    private sealed class FixedRandom(int value = 0) : IRandomProvider
    {
        public int Value { get; set; } = value;
        public int Next(int min, int max) => Value;
    }

    private static GameSession Create(int face = 0)
    {
        var game = new GameSession(1, new DiceFactory(new FixedRandom(face)));
        game.AddPlayer(new Player(1, "One"));
        game.AddPlayer(new Player(2, "Two"));
        return game;
    }

    [Test]
    public void LifecycleRequiresCompletedRoundsAndResetsDice()
    {
        var game = Create();
        Assert.That(game.State, Is.EqualTo(GameState.Preparation));
        Assert.Throws<InvalidOperationException>(() => game.Roll(1));
        game.Start();
        Assert.That(game.RoundNumber, Is.EqualTo(1));
        Assert.Throws<InvalidOperationException>(() => game.StartNextRound());
        Assert.Throws<InvalidOperationException>(() => game.BeginScoring());
        Assert.Throws<InvalidOperationException>(() => game.AddPlayer(new Player(3, "Three")));
        foreach (var id in new[] { 1, 2 })
        {
            game.Roll(id);
            game.Roll(id);
            game.Roll(id);
            Assert.That(game.PlayerDecks[id].IsTurnCompleted, Is.True);
            Assert.Throws<InvalidOperationException>(() => game.Roll(id));
        }
        game.StartNextRound();
        Assert.That(game.RoundNumber, Is.EqualTo(2));
        Assert.That(game.PlayerDecks[1].RestRolls, Is.EqualTo(3));
        Assert.That(game.PlayerDecks[1].CurrentFaces, Is.Empty);
        foreach (var id in new[] { 1, 2 }) { game.Roll(id); game.FinishTurn(id); }
        game.BeginScoring();
        Assert.That(game.State, Is.EqualTo(GameState.Scoring));
        Assert.Throws<InvalidOperationException>(() => game.StartNextRound());
    }

    [Test]
    public void SkullDiceStayFrozenUntilNextRound()
    {
        var game = Create(5);
        game.Start();
        game.Roll(1);
        var deck = game.PlayerDecks[1];
        var skull = deck.FreezedDices.First();
        Assert.Throws<InvalidOperationException>(() => deck.Unfreeze(skull));
        game.FinishTurn(1);
        game.Roll(2);
        game.FinishTurn(2);
        game.StartNextRound();
        Assert.That(deck.FreezedDices, Is.Empty);
        Assert.That(deck.RollingDices.Count(), Is.EqualTo(4));
    }

    [Test]
    public void NewDiceJoinNextRoundAndNewRollCanExtendThreeRolls()
    {
        var random = new FixedRandom();
        var game = new GameSession(1, new DiceFactory(random));
        game.AddPlayer(new Player(1, "One"));
        game.AddPlayer(new Player(2, "Two"));
        game.Start();
        var deck = game.PlayerDecks[1];
        deck.AddDice(DiceType.Clergy);
        Assert.That(deck.Dices.Count(), Is.EqualTo(4));
        foreach (var id in new[] { 1, 2 }) { game.Roll(id); game.FinishTurn(id); }
        game.StartNextRound();
        Assert.That(deck.Dices.Count(), Is.EqualTo(5));
        random.Value = 2;
        game.Roll(1); game.Roll(1); game.Roll(1);
        Assert.That(deck.IsTurnCompleted, Is.False);
        Assert.That(deck.RestRolls, Is.Zero);
        var clergy = deck.Dices.Single(dice => dice.DiceType == DiceType.Clergy);
        var target = deck.Dices.First();
        random.Value = 0;
        deck.ExtraRoll(clergy, target);
        Assert.That(deck.IsTurnCompleted, Is.True);
        Assert.Throws<InvalidOperationException>(() => deck.ExtraRoll(clergy, target));
    }

    [Test]
    public void DepartedPlayerKeepsDeckButDoesNotBlockFutureRounds()
    {
        var game = Create();
        game.Start();
        var deck = game.PlayerDecks[1];
        game.LeavePlayer(1);
        game.Roll(2);
        game.FinishTurn(2);
        game.StartNextRound();
        Assert.That(game.PlayerDecks[1], Is.SameAs(deck));
        Assert.That(game.HasLeft(1), Is.True);
        Assert.That(game.RoundNumber, Is.EqualTo(2));
        Assert.Throws<InvalidOperationException>(() => game.Roll(1));
    }

    [Test]
    public void SuppliesAndDecksAreIndependentBetweenGames()
    {
        var first = Create();
        var second = Create();
        var building = first.BuildingSupply.Available.First();
        Assert.That(first.PlayerDecks[1].TakeBuilding(first.BuildingSupply, building), Is.True);
        Assert.That(first.PlayerDecks[2].TakeBuilding(first.BuildingSupply, building), Is.False);
        Assert.That(second.BuildingSupply.Available.Count, Is.EqualTo(first.BuildingSupply.Available.Count + 1));
        Assert.That(first.PlayerDecks[1].Goods.TryAdd(1), Is.True);
        Assert.That(second.PlayerDecks[1].Goods.Count, Is.Zero);
    }
}
