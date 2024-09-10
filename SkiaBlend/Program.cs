namespace SkiaBlend;

internal class Program
{
    private static void Main(string[] args)
    {
        _ = args;

        using Game game = new();
        game.Run();
    }
}
