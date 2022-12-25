namespace SimpleFarmingGame.Game
{
    internal interface IPlayerInput
    {
        float InputX { get; }
        float InputY { get; }
        bool InputDisable { set; }
    }
}