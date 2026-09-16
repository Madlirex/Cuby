namespace MadEngine;

class Program
{
    static void Main(string[] args)
    {
        using Game game = Game.Create("Cuby");
        game.Run();
    }
}