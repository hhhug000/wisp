using System.ComponentModel;
using System.Numerics;
using MoonSharp.Interpreter;
using Raylib_cs;

namespace wisp;

public class CodeRunner
{
    // spritesheet to be imported
    private static Texture2D spritesheet;
    // the map
    private static int[,] mapData;
    // sound effect storage
    private static Sound[] soundEffects = new Sound[64];
    // random generator
    private static Random random = new Random();
    
    // list of pressed buttons
    private static bool[] currentButtons = new bool[8];
    private static bool[] previousButtons = new bool[8];
    
    // UTILITIES
    private static float min(float a, float b) => Math.Min(a, b);
    private static float max(float a, float b) => Math.Max(a, b);

    private static float mid(float x, float y, float z)
    {
        if (x > y) (x, y) = (y, x);
        if (y > z) (y, z) = (z, y);
        if (x > y) (x, y) = (y, x);
        return y;
    }

    // angle * 2.0 * Math.PI is there so angles go from 0 to 1 instead of from 0 to 2Pi
    private static float cos(float angle) => (float)Math.Cos(angle * 2.0 * Math.PI);
    private static float sin(float angle) => (float)Math.Sin(-angle * 2.0 * Math.PI);
    private static float atan2(float x, float y) => (float)(Math.Atan2(-y, x) / (2.0 * Math.PI));
    private static float abs(float x) => Math.Abs(x);
    private static float sqrt(float x) => (float)Math.Sqrt(x);

    private static float rnd(object x = null)
    {
        if (x == null)
        {
            return (float)random.NextDouble();
        }
        else if (x is double d)
        {
            return (float)(random.NextDouble() * d);
        }
        else if (x is int i)
        {
            return (float)(random.NextDouble() * i);
        }
        else if (x is MoonSharp.Interpreter.Table t)
        {
            int idx = random.Next(1, t.Values.Count() + 1);
            return (float)(t.Get(idx).Number);
        }
        return 0f;
    }

    private static void srand(int seed)
    {
        random = new Random(seed);
    }
    
    // gets the tile id at a map coord
    private static int mget(int x, int y)
    {
        if (mapData == null) return 0;
        if (x < 0 || x >= mapData.GetLength(0) || y < 0 || y >= mapData.GetLength(1)) return 0;
        return mapData[x, y];
    }
    
    // GRAPHICS LIBRARY
    private static void cls(int colorCode)
    {
        Raylib.ClearBackground(Constants.Colors[colorCode]);
    }
    
    private static void pset(int x, int y, int colorCode)
    {
        Raylib.DrawPixel(x, y, Constants.Colors[colorCode]);
    }

    private static void line(int x0, int y0, int x1, int y1, int colorCode)
    {
        Raylib.DrawLine(x0, y0, x1, y1, Constants.Colors[colorCode]);
    }

    private static void rect(int x, int y, int w, int h, int colorCode)
    {
        Raylib.DrawRectangleLines(x, y, w, h, Constants.Colors[colorCode]);
    }

    private static void rectfill(int x, int y, int w, int h, int colorCode)
    {
        Raylib.DrawRectangle(x, y, w, h, Constants.Colors[colorCode]);
    }

    private static void circ(int x, int y, int r, int colorCode)
    {
        Raylib.DrawCircleLines(x, y, r, Constants.Colors[colorCode]);
    }

    private static void circfill(int x, int y, int r, int colorCode)
    {
        Raylib.DrawCircle(x, y, r, Constants.Colors[colorCode]);
    }

    private static void print(string text, int x, int y, int colorCode)
    {
        Raylib.DrawText(text, x, y, 10, Constants.Colors[colorCode]);
    }

    private static void spr(int n, int x, int y, int? w = null, int? h = null, bool? flipX = null, bool? flipY = null)
    {
        int widthTiles = w ?? 1;
        int heightTiles = h ?? 1;
        bool fx = flipX ?? false;
        bool fy = flipY ?? false;

        int tilesPerRow = 16; // 128 / 8px per tile
        float tileSize = 8.0f;

        int srcX = (n % tilesPerRow) * (int)tileSize;
        int srcY = (n / tilesPerRow) * (int)tileSize;

        float sourceWidth = widthTiles * tileSize;
        float sourceHeight = heightTiles * tileSize;

        if (fx) sourceWidth = -sourceWidth;
        if (fy) sourceHeight = -sourceHeight;

        Rectangle sourceRec = new Rectangle(srcX, srcY, sourceWidth, sourceHeight);
        Rectangle destRec = new Rectangle(x, y, widthTiles * tileSize, heightTiles * tileSize);
        Vector2 origin = Vector2.Zero;
        
        Raylib.DrawTexturePro(spritesheet, sourceRec, destRec, origin, 0.0f, Color.White);
    }

    private static void map(int tileX, int tileY, int? sx = null, int? sy = null, int? tileW = null, int? tileH = null, object layers = null)
    {
        if (mapData == null) return;

        int screenX = sx ?? 0;
        int screenY = sy ?? 0;

        int mapCols = mapData.GetLength(0);
        int mapRows = mapData.GetLength(1);

        int widthInTiles = tileW ?? (mapCols - tileX);
        int heightInTiles = tileH ?? (mapRows - tileY);

        // one tile is 8x8px
        int tileSize = 8;

        for (int ty = 0; ty < heightInTiles; ty++)
        {
            for (int tx = 0; tx < widthInTiles; tx++)
            {
                int mx = tileX + tx;
                int my = tileY + ty;

                if (mx >= 0 && mx < mapCols && my >= 0 && my < mapRows)
                {
                    int tileId = mapData[mx, my];

                    // skip tile 0 if its empty
                    if (tileId > 0)
                    {
                        float drawX = screenX + (tx * tileSize);
                        float drawY = screenY + (ty * tileSize);
                        
                        spr(tileId, (int)drawX, (int)drawY);
                    }
                }
            }
        }
    }
    
    // INPUTS
    private static bool btn(int i)
    {
        if (i < 0 || i >= currentButtons.Length) return false;
        return currentButtons[i];
    }
    
    private static bool btnp(int i)
    {
        if (i < 0 || i >= currentButtons.Length) return false;
        return currentButtons[i] && !previousButtons[i];
    }

    private static object mousex()
    {
        if (!Raylib.IsCursorOnScreen()) return null;
        Vector2 vm = Program.GetVirtualMousePosition();
        if (vm.X >= 0 & vm.X < Constants.VirtualWidth && vm.Y >= 0 && vm.Y < Constants.VirtualHeight)
        {
            return (int)vm.X;
        }

        return null;
    }
    
    private static object mousey()
    {
        if (!Raylib.IsCursorOnScreen()) return null;
        Vector2 vm = Program.GetVirtualMousePosition();
        if (vm.X >= 0 & vm.X < Constants.VirtualWidth && vm.Y >= 0 && vm.Y < Constants.VirtualHeight)
        {
            return (int)vm.Y;
        }

        return null;
    }
    
    // SOUND EFFECTS
    private static void sfx(int n)
    {
        if (n >= 0 && n < soundEffects.Length && soundEffects[n].FrameCount > 0)
        {
            Raylib.PlaySound(soundEffects[n]);
        }
    }
    
    public static void Run(string code, RenderTexture2D texture, Texture2D spritesheetTexture, int[,] mapTiles, Sound[] sfxArray)
    {
        // save the spritesheetTexture to spritesheet so it can be used in the spr func
        spritesheet = spritesheetTexture;
        mapData = mapTiles;
        soundEffects = sfxArray;
        
        Script script = new Script();
        
        // register all the functions for use in the user code

        // UTILITIES
        script.Globals["min"] = (Func<float, float, float>)min;
        script.Globals["max"] = (Func<float, float, float>)max;
        script.Globals["mid"] = (Func<float, float, float, float>)mid;
        script.Globals["cos"] = (Func<float, float>)cos;
        script.Globals["sin"] = (Func<float, float>)sin;
        script.Globals["atan2"] = (Func<float, float, float>)atan2;
        script.Globals["abs"] = (Func<float, float>)abs;
        script.Globals["sqrt"] = (Func<float, float>)sqrt;
        script.Globals["rnd"] = (Func<object, float>)rnd;
        script.Globals["srand"] = (Action<int>)srand;
        script.Globals["mget"] = (Func<int, int, int>)mget;
        // GRAPHICS LIBRARY
        script.Globals["cls"] = (Action<int>)cls;
        script.Globals["pset"] = (Action<int, int, int>)pset;
        script.Globals["line"] = (Action<int, int, int, int, int>)line;
        script.Globals["rect"] = (Action<int, int, int, int, int>)rect;
        script.Globals["rectfill"] = (Action<int, int, int, int, int>)rectfill;
        script.Globals["circ"] = (Action<int, int, int, int>)circ;
        script.Globals["circfill"] = (Action<int, int, int, int>)circfill;
        script.Globals["print"] = (Action<string, int, int, int>)print;
        script.Globals["spr"] = (Action<int, int, int, int?, int?, bool?, bool?>)spr;
        script.Globals["map"] = (Action<int, int, int?, int?, int?, int?, object>)map;
        // INPUTS
        script.Globals["btn"] = (Func<int, bool>)btn;
        script.Globals["btnp"] = (Func<int, bool>)btnp;
        script.Globals["mousex"] = (Func<object>)mousex;
        script.Globals["mousey"] = (Func<object>)mousey;
        // SOUND EFFECTS
        script.Globals["sfx"] = (Action<int>)sfx;
        
        // from here on it is just running the code
        // this bit makes sure that you can draw
        Raylib.BeginTextureMode(texture);
        
        // this runs the whole code initially
        script.DoString(code);
        
        // this runs the _init function on start
        DynValue initFunc = script.Globals.Get("_init");
        // the if is to stop it crashing if a function is not implemented
        if (initFunc.Type == DataType.Function)
        {
            script.Call(initFunc);
        }
        
        Raylib.EndTextureMode();
        Program.DrawTextureToWindow(texture);
        
        DynValue updateFunc = script.Globals.Get("_update");
        DynValue drawFunc = script.Globals.Get("_draw");

        // runs forever until close
        while (!Raylib.WindowShouldClose())
        {
            Program.HandleWindowResizeAndFullscreen();
            
            // move all current buttons to previous buttons
            for (int i = 0; i < currentButtons.Length; i++)
            {
                previousButtons[i] = currentButtons[i];
            }
            
            // poll for keyboard and mouse buttons and move them to the currentButtons array
            // 0 is left, 1 is right, 2 is up, 3 is down, 4 is z, 5 is x, 6 is left click, 7 is right click
            currentButtons[0] = Raylib.IsKeyDown(KeyboardKey.Left) || Raylib.IsKeyDown(KeyboardKey.A);
            currentButtons[1] = Raylib.IsKeyDown(KeyboardKey.Right) || Raylib.IsKeyDown(KeyboardKey.D);
            currentButtons[2] = Raylib.IsKeyDown(KeyboardKey.Up) || Raylib.IsKeyDown(KeyboardKey.W);
            currentButtons[3] = Raylib.IsKeyDown(KeyboardKey.Down) || Raylib.IsKeyDown(KeyboardKey.S);
            currentButtons[4] = Raylib.IsKeyDown(KeyboardKey.Z);
            currentButtons[5] = Raylib.IsKeyDown(KeyboardKey.X);
            currentButtons[6] = Raylib.IsMouseButtonDown(MouseButton.Left);
            currentButtons[7] = Raylib.IsMouseButtonDown(MouseButton.Right);
            
            Raylib.BeginTextureMode(texture);
            if (updateFunc.Type == DataType.Function)
            {
                script.Call(updateFunc);
            }
            if (drawFunc.Type == DataType.Function)
            {
                script.Call(drawFunc);
            }
            Raylib.EndTextureMode();
            Program.DrawTextureToWindow(texture);
        }
    }
}