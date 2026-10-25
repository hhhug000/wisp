using System.Data.Common;
using System.Globalization;
using System.Numerics;
using MoonSharp.Interpreter;
using Raylib_cs;

namespace wisp;

public static class Constants
{
    public static readonly Color[] Colors = new Color[]
    {
        new Color(0, 0, 0, 255), // Black
        new Color(29, 43, 83, 255), // Dark Blue
        new Color(126, 37, 83, 255), // Purple
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
    
    public const int VirtualWidth = 128;
    public const int VirtualHeight = 128;
}

internal class Program
{
    public static void Main(string[] args)
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        
        // bigger so it looks good on screen, render texture will be 128 so it doesn't matter
        Raylib.InitWindow(512, 512, "Wisp");
        Raylib.SetTargetFPS(60);
        

        // create render texture, this is where stuff is drawn to then scaled to the main screen
        RenderTexture2D target = Raylib.LoadRenderTexture(Constants.VirtualWidth, Constants.VirtualHeight);

        Raylib.SetTextureFilter(target.Texture, TextureFilter.Point);

        PlayIntroAnimation(target);

        string code = @"
            local timer = 0
        
            function _update()
                timer = timer + 1
            end
        
            function _draw()
                if (timer < 60) then
                    cls(0)
                else
                    cls(5)
                end
                
                if (timer >= 120) then
                    timer = 0
                end
            end
        ";

        CodeRunner.Run(code, target);
        
        while (!Raylib.WindowShouldClose())
        {

            HandleWindowResizeAndFullscreen();

            Vector2 virtualMouse = GetVirtualMousePosition();

            bool isInsideCanvas = Raylib.IsCursorOnScreen() && virtualMouse.X >= 0 && virtualMouse.X < Constants.VirtualWidth && virtualMouse.Y >= 0 && virtualMouse.Y < Constants.VirtualHeight;

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

            DrawTextureToWindow(target);
        }

        // clean up gpu mem
        Raylib.UnloadRenderTexture(target);
        Raylib.CloseWindow();
    }

    public static void DrawTextureToWindow(RenderTexture2D target)
    {
        Raylib.BeginDrawing();

        // for letterbox bars, not bg color, texture will keep its bg
        Raylib.ClearBackground(Color.Black);

        float screenWidth = Raylib.GetScreenWidth();
        float screenHeight = Raylib.GetScreenHeight();

        // finds the scale factor whilst keeping aspect ratio
        float scale = Math.Min(screenWidth / Constants.VirtualWidth, screenHeight / Constants.VirtualHeight);

        // find coords for center positioning
        float destWidth = Constants.VirtualWidth * scale;
        float destHeight = Constants.VirtualHeight * scale;
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
    private static Vector2 GetVirtualMousePosition()
    {
        float screenWidth = Raylib.GetScreenWidth();
        float screenHeight = Raylib.GetScreenHeight();

        float scale = Math.Min(screenWidth / Constants.VirtualWidth, screenHeight / Constants.VirtualHeight);

        float destWidth = Constants.VirtualWidth * scale;
        float destHeight = Constants.VirtualHeight * scale;
        float destX = (screenWidth - destWidth) * 0.5f;
        float destY = (screenHeight - destHeight) * 0.5f;

        Vector2 rawMouse = Raylib.GetMousePosition();

        // subtract letterbox offset and divide by scale
        float virtualX = (rawMouse.X - destX) / scale;
        float virtualY = (rawMouse.Y - destY) / scale;

        return new Vector2(virtualX, virtualY);
    }

    // helper to keep aspect ratio on resize and let user fullscreen
    public static void HandleWindowResizeAndFullscreen()
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

    private static void PlayIntroAnimation(RenderTexture2D target)
    {
        Raylib.BeginTextureMode(target);
        Raylib.ClearBackground(Constants.Colors[0]);
        Raylib.EndTextureMode();
        DrawTextureToWindow(target);

        int i = 0;

        while (i < 8)
        {
            HandleWindowResizeAndFullscreen();
            Raylib.BeginTextureMode(target);
            Raylib.WaitTime(0.05);
            Raylib.DrawRectangle(0, i*16, 128, 16, Constants.Colors[i+5]);
            Raylib.EndTextureMode();
            DrawTextureToWindow(target);
            i++;
        }

        i = 0;

        while (i < 8)
        {
            HandleWindowResizeAndFullscreen();
            Raylib.BeginTextureMode(target);
            Raylib.WaitTime(0.05);
            Raylib.DrawRectangle(0, 128-(i*16), 128, 16, Constants.Colors[0]);
            Raylib.EndTextureMode();
            DrawTextureToWindow(target);
            i++;
        }
    }
}