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

        while (!Raylib.WindowShouldClose())
        {

            // this bit forces the window to be a square
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


            Raylib.BeginTextureMode(target);
            Raylib.ClearBackground(Color.Black);

            Raylib.DrawText("Hello from wisp!", 0, 0, 10, Color.White);

            Raylib.EndTextureMode();

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

            // get mouse position and scale it to canvas
            Vector2 rawMouse = Raylib.GetMousePosition();

            float relX = rawMouse.X - destX;
            float relY = rawMouse.Y - destY;

            float virtualMouseX = relX / scale;
            float virtualMouseY = relY / scale;

            bool isInsideCanvas = virtualMouseX >= 0 && virtualMouseX < virtualWidth && virtualMouseY >= 0 && virtualMouseY < virtualHeight;

            // Source and dest rec
            Rectangle sourceRec = new Rectangle(0.0f, 0.0f, (float)target.Texture.Width, -(float)target.Texture.Height);
            Rectangle destRec = new Rectangle(destX, destY, destWidth, destHeight);

            // draw texture
            Raylib.DrawTexturePro(target.Texture, sourceRec, destRec, Vector2.Zero, 0.0f, Color.White);

            Raylib.EndDrawing();
        }

        // clean up gpu mem
        Raylib.UnloadRenderTexture(target);
        Raylib.CloseWindow();
    }
}