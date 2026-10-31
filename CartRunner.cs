using System.Collections.Generic;
using Raylib_cs;

namespace wisp;

public class CartRunner
{
    public static void RunCart(string cartText, RenderTexture2D texture)
    {
        Dictionary<string, string> cartContent = FileParser.SplitToSections(cartText);
        Texture2D spritesheet = FileParser.LoadSpriteSheet(cartContent["sprites"]);

        int[,] map = cartContent.ContainsKey("map")
            ? FileParser.ParseMap(cartContent["map"])
            : new int[0, 0];
        
        CodeRunner.Run(cartContent["code"], texture, spritesheet, map);
    }
}