using System.Data.Common;
using System.Globalization;
using System.Numerics;
using Raylib_cs;

namespace wisp;

internal class Program
{
    public static void Main(string[] args)
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        
        // bigger so it looks good on screen, render texture will be 128 so it doesnt matter
        Raylib.InitWindow(512, 512, "Wisp");
        Raylib.SetTargetFPS(60);
        

        // create render texture, this is where stuff is drawn to then scaled to the main screen
        const int virtualWidth = 128;
        const int virtualHeight = 128;
        RenderTexture2D target = Raylib.LoadRenderTexture(virtualWidth, virtualHeight);

        Raylib.SetTextureFilter(target.Texture, TextureFilter.Point);

        Color[] Colors = new Color[]
        {
            new Color(0, 0, 0, 255), // Black
            new Color(29, 43, 83, 255), // Dark Blue
            new Color(126, 37, 83, 255), // Plum
            new Color(0, 135, 81, 255), // Dark Green
            new Color(171, 82, 54, 255), // Brown
            new Color(95, 87, 79, 255), // Grey
            new Color(194, 195, 199, 255), // Light Grey
            new Color(255, 241, 232, 255), // White
            new Color(255, 0, 77, 255), // Red
            new Color(255, 163, 0, 255), // Orange
            new Color(255, 236, 39, 255), // Yellow
            new Color(0, 228, 54, 255), // Green
            new Color(41, 173, 255, 255) // Blue
        };

        PlayIntroAnimation(virtualWidth, virtualHeight, target, Colors);

        while (!Raylib.WindowShouldClose())
        {

            HandleWindowResizeAndFullscreen();

            Vector2 virtualMouse = GetVirtualMousePosition(virtualWidth, virtualHeight);

            bool isInsideCanvas = Raylib.IsCursorOnScreen() && virtualMouse.X >= 0 && virtualMouse.X < virtualWidth && virtualMouse.Y >= 0 && virtualMouse.Y < virtualHeight;

            if (isInsideCanvas && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                Console.WriteLine($"Clicked at x{(int)virtualMouse.X} y{(int)virtualMouse.Y}");
            }

            Raylib.BeginTextureMode(target);
            Raylib.ClearBackground(Color.Black);

            Raylib.DrawText("Hello from wisp!", 0, 0, 10, Color.White);

            if (isInsideCanvas)
            {
                Raylib.DrawRectangle((int)virtualMouse.X - 1, (int)virtualMouse.Y - 1, 3, 3, Color.Red);
            }

            Raylib.EndTextureMode();

            DrawTextureToWindow(virtualWidth, virtualHeight, target);
        }

        // clean up gpu mem
        Raylib.UnloadRenderTexture(target);
        Raylib.CloseWindow();
    }

    private static void DrawTextureToWindow(int virtualWidth, int virtualHeight, RenderTexture2D target)
    {
        Raylib.BeginDrawing();

        // for letterbox bars, not bg color, texture will keep its bg
        Raylib.ClearBackground(Color.Black);

        float screenWidth = Raylib.GetScreenWidth();
        float screenHeight = Raylib.GetScreenHeight();

        // finds the scale factor whilst keeping aspect ratio
        float scale = Math.Min(screenWidth / virtualWidth, screenHeight / virtualHeight);

        // find coords for center positioning
        float destWidth = virtualWidth * scale;
        float destHeight = virtualHeight * scale;
        float destX = (screenWidth - destWidth) * 0.5f;
        float destY = (screenHeight - destHeight) * 0.5f;

        // Source and dest rec
        Rectangle sourceRec = new Rectangle(0.0f, 0.0f, (float)target.Texture.Width, -(float)target.Texture.Height);
        Rectangle destRec = new Rectangle(destX, destY, destWidth, destHeight);

        // draw texture
        Raylib.DrawTexturePro(target.Texture, sourceRec, destRec, Vector2.Zero, 0.0f, Color.White);

        Raylib.EndDrawing();
    }

    // helper func to map mouse position to canvas space
    private static Vector2 GetVirtualMousePosition(int virtualWidth, int virtualHeight)
    {
        float screenWidth = Raylib.GetScreenWidth();
        float screenHeight = Raylib.GetScreenHeight();

        float scale = Math.Min(screenWidth / virtualWidth, screenHeight / virtualHeight);

        float destWidth = virtualWidth * scale;
        float destHeight = virtualHeight * scale;
        float destX = (screenWidth - destWidth) * 0.5f;
        float destY = (screenHeight - destHeight) * 0.5f;

        Vector2 rawMouse = Raylib.GetMousePosition();

        // subtract letterbox offset and divide by scale
        float virtualX = (rawMouse.X - destX) / scale;
        float virtualY = (rawMouse.Y - destY) / scale;

        return new Vector2(virtualX, virtualY);
    }

    // helper to keep aspect ratio on resize and let user fullscreen
    private static void HandleWindowResizeAndFullscreen()
    {
        if (!Raylib.IsWindowFullscreen())
        {
            int currentWidth = Raylib.GetScreenWidth();
            int currentHeight = Raylib.GetScreenHeight();

            if (currentWidth != currentHeight)
            {
                int size = Math.Min(currentWidth, currentHeight);
                Raylib.SetWindowSize(size, size);
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.F11))
        {
            Raylib.ToggleFullscreen();
        }
    }

    public static void PlayIntroAnimation(int virtualWidth, int virtualHeight, RenderTexture2D target, Color[] Colors)
    {
        Raylib.BeginTextureMode(target);
        Raylib.ClearBackground(Color.Black);
        Raylib.EndTextureMode();
        DrawTextureToWindow(virtualWidth, virtualHeight, target);

        int i = 0;

        while (i < 8)
        {
            HandleWindowResizeAndFullscreen();
            Raylib.BeginTextureMode(target);
            Raylib.WaitTime(0.05);
            Raylib.DrawRectangle(0, i*16, 128, 16, Colors[i+5]);
            Raylib.EndTextureMode();
            DrawTextureToWindow(virtualWidth, virtualHeight, target);
            i++;
        }

        i = 0;

        while (i < 8)
        {
            HandleWindowResizeAndFullscreen();
            Raylib.BeginTextureMode(target);
            Raylib.WaitTime(0.05);
            Raylib.DrawRectangle(0, 128-(i*16), 128, 16, Colors[0]);
            Raylib.EndTextureMode();
            DrawTextureToWindow(virtualWidth, virtualHeight, target);
            i++;
        }
    }
}