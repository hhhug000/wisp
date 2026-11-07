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

        Sound[] sfx = cartContent.ContainsKey("sfx")
            ? FileParser.LoadSoundEffects(cartContent["sfx"])
            : new Sound[64];

        List<FileParser.MusicRow> musicTrack = cartContent.ContainsKey("music")
            ? FileParser.ParseMusic(cartContent["music"])
            : new List<FileParser.MusicRow>();
        
        CodeRunner.Run(cartContent["code"], texture, spritesheet, map, sfx, musicTrack);

        foreach (var sound in sfx)
        {
            if (sound.FrameCount > 0)
            {
                Raylib.UnloadSound(sound);
            }
        }
    }
}