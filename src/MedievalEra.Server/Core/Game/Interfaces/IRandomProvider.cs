namespace MedievalEra.Server.Core.Game.Interfaces
{
    public interface IRandomProvider
    {
        int Next(int min, int max);
    }
}
