using MedievalEra.Server.Core.Game.Dice;
using MedievalEra.Server.Core.Game.Enums;
using MedievalEra.Server.Core.Game.Interfaces;

namespace MedievalEra.Server.Core.Game.Deck
{
    public class PlayerDeck
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

        private void InitializeBoard()
        {
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
        public PlayerDeck(string playerName, DiceFactory diceFactory)
        {
            InitializeBoard();
            PlayerName = playerName;
            _diceFactory = diceFactory;
            _rollingDices.AddRange(_diceFactory.GetStarterKit());
        }

        /// <summary>
        /// Восстановление из хранилища
        /// </summary>
        /// <param name="data"></param>
        public PlayerDeck(object data)
        {
            throw new NotImplementedException("Восстановление доски из хранилища пока не реализовано.");
        }
        public string PlayerName { get; }
        public Dictionary<GamingSource, int> Resources { get; } = new()
        {
            { GamingSource.Goods, 0},
            { GamingSource.Stone, 1},
            { GamingSource.Wood, 2},
            { GamingSource.Meal, 3}
        };
        public int Skulls { get; }
        public int Culture { get; }
        public int RestRolls { get; } = TotalRolls;
        public IEnumerable<IDice> Dices => RollingDices.Union(FreezedDices);
        public IEnumerable<IDice> RollingDices => _rollingDices;
        public IEnumerable<IDice> FreezedDices => _freezedDices;

        public void Roll()
        {

        }

        public void Finish()
        {

        }
    }
}
