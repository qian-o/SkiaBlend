namespace SkiaBlend;

internal class Program
{
    static void Main(string[] args)
    {
        _ = args;

        using Game game = new();
        game.Run();
    }
}
