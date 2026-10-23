using Raylib_cs;

namespace wisp;

internal class Program
{
    public static void Main(string[] args)
    {
        Raylib.InitWindow(800, 450, "Wisp");
        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            Raylib.DrawText("Hello from wisp!", 0, 0, 20, Color.White);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}