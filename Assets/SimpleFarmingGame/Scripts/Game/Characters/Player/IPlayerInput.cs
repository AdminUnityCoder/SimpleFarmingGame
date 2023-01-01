namespace SimpleFarmingGame.Game
{
    public interface IPlayerInput
    {
        float InputX { get; }
        float InputY { get; }
        bool InputDisable { set; }
    }
}