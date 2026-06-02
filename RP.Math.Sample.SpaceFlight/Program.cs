namespace RP.Math.Sample.SpaceFlight
{
    internal static class Program
    {
        [System.STAThread]
        private static void Main()
        {
            using var game = new SpaceGame();
            game.Run();
        }
    }
}
