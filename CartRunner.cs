using System.Collections.Generic;
using Raylib_cs;

namespace wisp;

public class CartRunner
{
    public static void RunCart(string cartText, RenderTexture2D texture)
    {
        Dictionary<string, string> cartContent = FileParser.SplitToSections(cartText);
        Texture2D spritesheet = FileParser.LoadSpriteSheet(cartContent["sprites"]);
        CodeRunner.Run(cartContent["code"], texture, spritesheet);
    }
}