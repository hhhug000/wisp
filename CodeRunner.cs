using MoonSharp.Interpreter;
using Raylib_cs;

namespace wisp;

public class CodeRunner
{
    private static void cls(int colorCode)
    {
        Raylib.ClearBackground(Constants.Colors[colorCode]);
    }

    public static void Run(string code, RenderTexture2D texture)
    {
        Script script = new Script();
        
        // register all the functions for use in the user code
        script.Globals["cls"] = (Action<int>)cls;
        
        // from here on it is just running the code
        // this runs the whole code initially
        script.DoString(code);
        
        // this runs the _init function on start
        DynValue initFunc = script.Globals.Get("_init");
        // the if is to stop it crashing if a function is not implemented
        if (initFunc.Type == DataType.Function)
        {
            script.Call(initFunc);
        }
        DynValue updateFunc = script.Globals.Get("_update");
        DynValue drawFunc = script.Globals.Get("_draw");

        // runs forever until close
        while (!Raylib.WindowShouldClose())
        {
            Program.HandleWindowResizeAndFullscreen();
            if (updateFunc.Type == DataType.Function)
            {
                script.Call(updateFunc);
            }
            if (drawFunc.Type == DataType.Function)
            {
                Raylib.BeginTextureMode(texture);
                script.Call(drawFunc);
                Raylib.EndTextureMode();
                Program.DrawTextureToWindow(texture);
            }
        }
    }
}