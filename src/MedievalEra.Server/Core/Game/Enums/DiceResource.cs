namespace MedievalEra.Server.Core.Game.Enums
{
    [Flags]
    public enum DiceResource
    {
        None = 0,
        Goods = 1,
        Stone = 2,
        Wood = 4,
        Meal = 8,
        Skull = 16,
        Culture = 32,
        Attack = 64,
        Defence = 128,
        Building = 256,
        NewRoll = 512
    }
}
