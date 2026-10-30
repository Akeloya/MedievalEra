using MedievalEra.Server.Core.Game.Common;
using MedievalEra.Server.Core.Game.Deck;
using MedievalEra.Server.Core.Game.Dice;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;
using MedievalEra.Server.Core.Game.Player;

namespace MedievalEra.Server.Tests.Core.Game;

public class PlayerDeckResourceTests
{
    [Test]
    public void InitializesPlayerAndResourceTracks()
    {
        var player = new Player(7, "Player");
        IPlayerDeck deck = new PlayerDeck(player, new DiceFactory(new DefaultRandomProvider()));

        Assert.That(deck.Player, Is.SameAs(player));
        Assert.That(deck.GameResources, Is.EqualTo(new[] { deck.Goods, deck.Stone, deck.Wood, deck.Meal }));
        var resources = new[] { deck.Skull, deck.Culture, deck.Goods, deck.Stone, deck.Wood, deck.Meal };
        Assert.That(resources.Select(resource => resource.Count), Is.EqualTo(new[] { 0, 0, 0, 1, 2, 3 }));
        Assert.That(resources.Select(resource => resource.MaxValue), Is.EqualTo(new[] { 25, 25, 6, 7, 8, 10 }));
        Assert.That(resources.Select(resource => resource.Resource), Is.EqualTo(new[]
        {
            GamingSource.Skull, GamingSource.Culture, GamingSource.Goods,
            GamingSource.Stone, GamingSource.Wood, GamingSource.Meal
        }));
    }

    [Test]
    public void ResourceRejectsOutOfRangeChangesWithoutChangingCount()
    {
        IResource resource = new GameResource(GamingSource.Stone, 1, 7);
        Assert.That(resource.TryAdd(6), Is.True);
        Assert.That(resource.TryAdd(1), Is.False);
        Assert.That(resource.TryAdd(int.MaxValue), Is.False);
        Assert.That(resource.TryAdd(-1), Is.False);
        Assert.That(resource.Count, Is.EqualTo(7));
        Assert.That(resource.TryDecrase(8), Is.False);
        Assert.That(resource.TryDecrase(-1), Is.False);
        Assert.That(resource.TryDecrase(7), Is.True);
        Assert.That(resource.Count, Is.Zero);
    }
}
