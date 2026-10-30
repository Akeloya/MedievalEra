using MedievalEra.Server.Core.Game.Dice;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Deck
{
    public class PlayerDeck : IPlayerDeck
    {
        public const int BoardSize = 14;
        private readonly int?[,] _field = new int?[BoardSize, BoardSize];
        private readonly Dictionary<int, IBuilding> _buildings = [];
        private readonly List<IBuilding> _unbuiltBuildings = [];
        private readonly Dictionary<int, (int X, int Y)[]> _occupiedCells = [];
        private int _nextBuildingId = 1;

        public int?[,] Field => (int?[,])_field.Clone();
        public IReadOnlyDictionary<int, IBuilding> Buildings =>
            new System.Collections.ObjectModel.ReadOnlyDictionary<int, IBuilding>(_buildings);
        public IReadOnlyList<IBuilding> UnbuiltBuildings => _unbuiltBuildings.AsReadOnly();
        public IPlayer Player { get; }
        public IResource Goods { get; private set; } = null!;
        public IResource Stone { get; private set; } = null!;
        public IResource Wood { get; private set; } = null!;
        public IResource Meal { get; private set; } = null!;
        public IReadOnlyCollection<IResource> GameResources { get; private set; } = null!;
        public IResource Skull { get; private set; } = null!;
        public IResource Culture { get; private set; } = null!;

        private void InitializeBoard()
        {
            // Начальные значения ресурсов и пределы шкал планшета.
            Skull = new GameResource(GamingSource.Skull, 0, 25);
            Culture = new GameResource(GamingSource.Culture, 0, 25);
            Goods = new GameResource(GamingSource.Goods, 0, 6);
            Stone = new GameResource(GamingSource.Stone, 1, 7);
            Wood = new GameResource(GamingSource.Wood, 2, 8);
            Meal = new GameResource(GamingSource.Meal, 3, 10);
            GameResources = Array.AsReadOnly<IResource>([Goods, Stone, Wood, Meal]);

            var tower = new Building(Enums.Buildings.Tower, "Башня", 1, 1, DiceType.Nobility);
            tower.IsPlaced = true;
            _buildings.Add(0, tower);
            _occupiedCells.Add(0, [(BoardSize / 2, BoardSize / 2)]);
            _field[BoardSize / 2, BoardSize / 2] = 0;
        }

        public bool TakeBuilding(BuildingSupply supply, IBuilding building)
        {
            if (_unbuiltBuildings.Any(b => ReferenceEquals(b, building)) ||
                _buildings.Values.Any(b => ReferenceEquals(b, building)) || !supply.Take(building))
                return false;
            _unbuiltBuildings.Add(building);
            return true;
        }

        public bool ReturnBuilding(BuildingSupply supply, IBuilding building)
        {
            var index = _unbuiltBuildings.FindIndex(b => ReferenceEquals(b, building));
            if (index < 0 || !supply.Return(building)) return false;
            _unbuiltBuildings.RemoveAt(index);
            return true;
        }

        public bool CanBuild(int x, int y, IBuilding building)
        {
            if (!_unbuiltBuildings.Any(b => ReferenceEquals(b, building))) return false;
            var mask = building.Mask;
            if (x < 0 || y < 0 || x > BoardSize - mask.Width || y > BoardSize - mask.Height)
                return false;
            for (var dx = 0; dx < mask.Width; dx++)
                for (var dy = 0; dy < mask.Height; dy++)
                    if (mask[dx, dy] && _field[x + dx, y + dy].HasValue) return false;
            return true;
        }

        public bool Build(int x, int y, IBuilding building)
        {
            if (!CanBuild(x, y, building)) return false;
            var cells = new List<(int X, int Y)>();
            var mask = building.Mask;
            for (var dx = 0; dx < mask.Width; dx++)
                for (var dy = 0; dy < mask.Height; dy++)
                    if (mask[dx, dy]) cells.Add((x + dx, y + dy));
            var id = _nextBuildingId++;
            foreach (var cell in cells) _field[cell.X, cell.Y] = id;
            _buildings.Add(id, building);
            if (building is Building instance) instance.IsPlaced = true;
            _occupiedCells.Add(id, cells.ToArray());
            _unbuiltBuildings.RemoveAt(_unbuiltBuildings.FindIndex(b => ReferenceEquals(b, building)));
            return true;
        }

        public bool RemoveBuilding(int id)
        {
            if (id == 0 || !_buildings.TryGetValue(id, out var building) || building.IsInsideWall)
                return false;
            foreach (var cell in _occupiedCells[id]) _field[cell.X, cell.Y] = null;
            _occupiedCells.Remove(id);
            _buildings.Remove(id);
            if (building is Building instance) instance.IsPlaced = false;
            _unbuiltBuildings.Add(building);
            return true;
        }

        /// <summary>
        /// Максимальное количество зданий, приносящих очки бедствий, в одном кластере.
        /// Соседство определяется общей стороной занятых клеток разных зданий.
        /// Стены и башни не участвуют; госпитали защищают только прямых соседей.
        /// </summary>
        public int CalculateBuildingClaster()
        {
            var neighbors = new Dictionary<int, HashSet<int>>();
            var protectedByHospital = new HashSet<int>();

            void Connect(int? first, int? second)
            {
                if (first is not int a || second is not int b || a == b) return;
                var firstType = _buildings[a].Type;
                var secondType = _buildings[b].Type;
                if (firstType is Enums.Buildings.Wall or Enums.Buildings.Tower ||
                    secondType is Enums.Buildings.Wall or Enums.Buildings.Tower) return;

                if (!neighbors.ContainsKey(a)) neighbors[a] = [];
                if (!neighbors.ContainsKey(b)) neighbors[b] = [];
                neighbors[a].Add(b);
                neighbors[b].Add(a);
                if (firstType == Enums.Buildings.Hospital) protectedByHospital.Add(b);
                if (secondType == Enums.Buildings.Hospital) protectedByHospital.Add(a);
            }

            // Каждую общую сторону проверяем один раз.
            for (var x = 0; x < BoardSize; x++)
                for (var y = 0; y < BoardSize; y++)
                {
                    if (x + 1 < BoardSize) Connect(_field[x, y], _field[x + 1, y]);
                    if (y + 1 < BoardSize) Connect(_field[x, y], _field[x, y + 1]);
                }

            var visited = new HashSet<int>();
            var pending = new Stack<int>();
            var maximum = 0;
            foreach (var start in neighbors.Keys)
            {
                if (!visited.Add(start)) continue;
                pending.Push(start);
                var count = 0;
                while (pending.TryPop(out var id))
                {
                    if (_buildings[id].Type != Enums.Buildings.Hospital &&
                        !protectedByHospital.Contains(id)) count++;
                    foreach (var neighbor in neighbors[id])
                        if (visited.Add(neighbor)) pending.Push(neighbor);
                }
                maximum = Math.Max(maximum, count);
            }
            return maximum;
        }

        public const int TotalRolls = 3;
        private readonly DiceFactory _diceFactory;
        private readonly List<IDice> _rollingDices = new(4);
        private readonly List<IDice> _freezedDices = new(4);
        private readonly List<IDice> _allDices = new(4);
        private readonly List<IDice> _newDices = [];

        public void AddDice(DiceType type) => _newDices.Add(_diceFactory.GetDice(type));
        public PlayerDeck(string playerName, DiceFactory diceFactory)
            : this(new Player.Player(0, playerName), diceFactory)
        {
        }

        public PlayerDeck(IPlayer player, DiceFactory diceFactory)
        {
            ArgumentNullException.ThrowIfNull(player);
            Player = player;
            InitializeBoard();
            _diceFactory = diceFactory;
            _rollingDices.AddRange(_diceFactory.GetStarterKit());
            _allDices.AddRange(_rollingDices);
        }

        /// <summary>
        /// Восстановление из хранилища
        /// </summary>
        /// <param name="data"></param>
        public PlayerDeck(object data)
        {
            throw new NotImplementedException("Восстановление доски из хранилища пока не реализовано.");
        }
        public string PlayerName => Player.Name;
        public IReadOnlyDictionary<GamingSource, int> Resources =>
            GameResources.ToDictionary(resource => resource.Resource, resource => resource.Count);
        public int Skulls => Skull.Count;
        public int RestRolls { get; private set; } = TotalRolls;
        public bool IsTurnCompleted { get; private set; } = true;
        private readonly Dictionary<IDice, IDiceFace> _currentFaces = new(ReferenceEqualityComparer.Instance);
        public IReadOnlyDictionary<IDice, IDiceFace> CurrentFaces =>
            new System.Collections.ObjectModel.ReadOnlyDictionary<IDice, IDiceFace>(_currentFaces);
        public IEnumerable<IDice> Dices => _allDices.AsReadOnly();
        public IEnumerable<IDice> RollingDices => _rollingDices.AsReadOnly();
        public IEnumerable<IDice> FreezedDices => _freezedDices.AsReadOnly();

        internal void StartRound()
        {
            _allDices.AddRange(_newDices);
            _rollingDices.AddRange(_newDices);
            _newDices.Clear();
            foreach (var dice in Dices) dice.Unlock();
            _rollingDices.AddRange(_freezedDices);
            _freezedDices.Clear();
            _currentFaces.Clear();
            RestRolls = TotalRolls;
            IsTurnCompleted = false;
        }

        public void Roll()
        {
            RequireActiveTurn();
            if (RestRolls == 0) throw new InvalidOperationException("Нет доступных бросков.");
            foreach (var dice in _rollingDices.ToArray())
            {
                _currentFaces[dice] = dice.Roll();
                if (HasResource(dice, DiceResource.Skull)) Freeze(dice);
            }
            RestRolls--;
            if (RestRolls == 0 && !_rollingDices.Any(dice => HasResource(dice, DiceResource.NewRoll)))
                Finish();
        }

        public void Freeze(IDice dice)
        {
            RequireActiveTurn();
            if (!_rollingDices.Contains(dice) || !_currentFaces.TryGetValue(dice, out var face))
                throw new InvalidOperationException("Кубик недоступен для фиксации.");
            dice.Lock(face);
            _rollingDices.Remove(dice);
            _freezedDices.Add(dice);
        }

        public void Unfreeze(IDice dice)
        {
            RequireActiveTurn();
            if (RestRolls == 0 || !_freezedDices.Contains(dice) || HasResource(dice, DiceResource.Skull))
                throw new InvalidOperationException("Кубик нельзя разморозить.");
            dice.Unlock();
            _freezedDices.Remove(dice);
            _rollingDices.Add(dice);
        }

        public void ExtraRoll(IDice clergy, IDice target)
        {
            RequireActiveTurn();
            if (RestRolls != 0 || ReferenceEquals(clergy, target) ||
                clergy.DiceType != DiceType.Clergy || !_rollingDices.Contains(clergy) ||
                !HasResource(clergy, DiceResource.NewRoll) || !Dices.Contains(target) ||
                !_currentFaces.ContainsKey(target) || HasResource(target, DiceResource.Skull))
                throw new InvalidOperationException("Недопустимая пара для дополнительного броска.");
            foreach (var dice in new[] { clergy, target })
            {
                dice.Unlock();
                _freezedDices.Remove(dice);
                if (!_rollingDices.Contains(dice)) _rollingDices.Add(dice);
                _currentFaces[dice] = dice.Roll();
                if (HasResource(dice, DiceResource.Skull)) Freeze(dice);
            }
            if (!HasResource(clergy, DiceResource.NewRoll))
                foreach (var dice in new[] { clergy, target })
                    if (_rollingDices.Contains(dice)) Freeze(dice);
            if (!_rollingDices.Any(dice => HasResource(dice, DiceResource.NewRoll))) Finish();
        }

        public void Finish()
        {
            RequireActiveTurn();
            if (_currentFaces.Count == 0) throw new InvalidOperationException("Сначала бросьте кубики.");
            foreach (var dice in _rollingDices.ToArray()) Freeze(dice);
            RestRolls = 0;
            IsTurnCompleted = true;
        }

        private bool HasResource(IDice dice, DiceResource resource) =>
            _currentFaces.TryGetValue(dice, out var face) && face.Values.GetValueOrDefault(resource) > 0;

        private void RequireActiveTurn()
        {
            if (IsTurnCompleted) throw new InvalidOperationException("Ход не начат или уже завершён.");
        }
    }
}
