using MoonSharp.Interpreter;
using Raylib_cs;

namespace wisp;

public class CodeRunner
{
    public static void Run(string code)
    {
        Script script = new Script();
        script.DoString(code);
        
        DynValue initFunc = script.Globals.Get("_init");
        if (initFunc.Type == DataType.Function)
        {
            script.Call(initFunc);
        }
        DynValue updateFunc = script.Globals.Get("_update");
        DynValue drawFunc = script.Globals.Get("_draw");

        while (!Raylib.WindowShouldClose())
        {
            if (updateFunc.Type == DataType.Function)
            {
                script.Call(updateFunc);
            }
            if (drawFunc.Type == DataType.Function)
            {
                script.Call(drawFunc);
            }
        }
    }
}