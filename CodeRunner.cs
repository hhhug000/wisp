using System.ComponentModel;
using MoonSharp.Interpreter;
using Raylib_cs;

namespace wisp;

public class CodeRunner
{
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
    
    public static void Run(string code, RenderTexture2D texture)
    {
        Script script = new Script();
        
        // register all the functions for use in the user code
        script.Globals["cls"] = (Action<int>)cls;
        script.Globals["pset"] = (Action<int, int, int>)pset;
        script.Globals["line"] = (Action<int, int, int, int, int>)line;
        script.Globals["rect"] = (Action<int, int, int, int, int>)rect;
        script.Globals["rectfill"] = (Action<int, int, int, int, int>)rectfill;
        script.Globals["circ"] = (Action<int, int, int, int>)circ;
        script.Globals["circfill"] = (Action<int, int, int, int>)circfill;
        script.Globals["print"] = (Action<string, int, int, int>)print;
        
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
            Raylib.BeginTextureMode(texture);
            Program.HandleWindowResizeAndFullscreen();
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