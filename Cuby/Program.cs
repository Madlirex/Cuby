namespace MadEngine;

class Program
{
    static void Main(string[] args)
    {
        using (Game game = new Game("Cuby"))
        {
            game.Run();
        }
    }
}