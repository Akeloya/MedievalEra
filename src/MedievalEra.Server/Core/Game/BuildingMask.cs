namespace MedievalEra.Server.Core.Game;

/// <summary>Неизменяемая прямоугольная маска. Координаты: [x, y].</summary>
public sealed class BuildingMask
{
    private readonly string[] rows;
    public int Width => rows[0].Length;
    public int Height => rows.Length;
    public bool this[int x, int y] => rows[y][x] == 'x';

    public BuildingMask(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        rows = pattern.Replace("\r\n", "\n").Split('\n');
        if (rows.Length == 0 || rows[0].Length == 0 ||
            rows.Any(row => row.Length != rows[0].Length || row.Any(c => c is not ('x' or 'o'))) ||
            !rows.Any(row => row.Contains('x')))
            throw new ArgumentException("Маска должна быть прямоугольником из x и o с хотя бы одной x.", nameof(pattern));
    }

    public BuildingMask RotateClockwise() => new(string.Join("\n",
        Enumerable.Range(0, Width).Select(y => new string(
            Enumerable.Range(0, Height).Select(x => rows[Height - 1 - x][y]).ToArray()))));

    public override string ToString() => string.Join("\n", rows);
}
