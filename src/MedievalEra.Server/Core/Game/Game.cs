using MedievalEra.Server.Core.Game.Deck;
using MedievalEra.Server.Core.Game.Dice;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game;

/// <summary>Одна партия с общим запасом зданий и индивидуальными планшетами.</summary>
public sealed class Game(int id, DiceFactory diceFactory)
{
    private readonly Dictionary<int, PlayerDeck> decks = [];
    private readonly HashSet<int> departedPlayers = [];
    public bool HasLeft(int playerId) => departedPlayers.Contains(playerId);

    public void LeavePlayer(int playerId)
    {
        if (!decks.ContainsKey(playerId)) throw new ArgumentOutOfRangeException(nameof(playerId));
        departedPlayers.Add(playerId);
    }
    public int Id { get; } = id;
    public GameState State { get; private set; } = GameState.Preparation;
    public int RoundNumber { get; private set; }
    public BuildingSupply BuildingSupply { get; } = new();
    public IReadOnlyDictionary<int, PlayerDeck> PlayerDecks =>
        new System.Collections.ObjectModel.ReadOnlyDictionary<int, PlayerDeck>(decks);
    public IReadOnlyCollection<IPlayer> Players => decks.Values.Select(deck => deck.Player).ToArray();

    public void AddPlayer(IPlayer player)
    {
        RequireState(GameState.Preparation);
        if (decks.Count >= 4) throw new InvalidOperationException("Все места заняты.");
        if (decks.ContainsKey(player.Id)) throw new InvalidOperationException("Игрок уже участвует в партии.");
        decks.Add(player.Id, new PlayerDeck(player, diceFactory));
    }

    public void RemovePlayer(int playerId)
    {
        RequireState(GameState.Preparation);
        decks.Remove(playerId);
    }

    public void Start()
    {
        RequireState(GameState.Preparation);
        if (decks.Count < 2) throw new InvalidOperationException("Нужны как минимум два игрока.");
        State = GameState.Playing;
        BeginRound();
    }

    public void Roll(int playerId) { RequireActivePlayer(playerId); decks[playerId].Roll(); }
    public void FinishTurn(int playerId) { RequireActivePlayer(playerId); decks[playerId].Finish(); }

    public void StartNextRound()
    {
        RequireState(GameState.Playing);
        if (decks.Any(entry => !HasLeft(entry.Key) && !entry.Value.IsTurnCompleted))
            throw new InvalidOperationException("Не все игроки завершили ход.");
        BeginRound();
    }

    public void BeginScoring()
    {
        RequireState(GameState.Playing);
        if (decks.Any(entry => !HasLeft(entry.Key) && !entry.Value.IsTurnCompleted))
            throw new InvalidOperationException("Сначала завершите текущий раунд.");
        State = GameState.Scoring;
    }

    private void BeginRound()
    {
        RoundNumber++;
        foreach (var entry in decks.Where(entry => !HasLeft(entry.Key))) entry.Value.StartRound();
    }

    private void RequireActivePlayer(int playerId)
    {
        RequireState(GameState.Playing);
        if (HasLeft(playerId)) throw new InvalidOperationException("Игрок покинул партию.");
    }

    private void RequireState(GameState state)
    {
        if (State != state) throw new InvalidOperationException("Действие недоступно в текущем состоянии игры.");
    }
}
