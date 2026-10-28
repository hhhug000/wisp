using System.Runtime.InteropServices;
using Raylib_cs;

namespace wisp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class FileParser
{
    // function to split a wisp file into the individual sections
    public static Dictionary<string, string> SplitToSections(string rawText)
    {
        // split the raw text into lines to be separated
        string[] lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        if (lines.Length < 2)
        {
            throw new InvalidDataException("Invalid cartridge (Too short to contain header)");
        }

        string header = lines[0].Trim();
        if (!header.Equals("wisp cartridge", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"Invalid cartridge (Expected 'wisp cartridge', got '{header}')");
        }

        string versionLine = lines[1].Trim();
        if (!versionLine.StartsWith("version ", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"Invalid cartridge (Expected 'version' header, got '{versionLine}')");
        }
        
        var sectionBuilders = new Dictionary<string, StringBuilder>();
        string currentSection = string.Empty;

        for (int i = 2; i < lines.Length; i++)
        {
            string rawLine = lines[i];
            string trimmed = rawLine.Trim();

            if (trimmed.StartsWith("__") && trimmed.EndsWith("__") && trimmed.Length > 4)
            {
                currentSection = trimmed.Substring(2, trimmed.Length - 4).ToLowerInvariant();

                if (!sectionBuilders.ContainsKey(currentSection))
                {
                    sectionBuilders[currentSection] = new StringBuilder();
                }

                continue;
            }

            if (!string.IsNullOrEmpty(currentSection))
            {
                sectionBuilders[currentSection].AppendLine(rawLine);
            }
        }
        
        var sections = new Dictionary<string, string>();
        foreach (var kvp in sectionBuilders)
        {
            sections[kvp.Key] = kvp.Value.ToString().TrimEnd('\r', '\n');
        }
        
        return sections;
    }

    // func to convert spritesheet in text format to a Raylib Texture2D
    public unsafe static Texture2D LoadSpriteSheet(string spriteText)
    {
        // split to lines for processing
        string[] lines = spriteText.Split(new[] { "\r\n", "\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);

        int width = 128;
        int height = 128;
        // all the pixels
        Color[] pixels = new Color[width * height];

        int pixelIndex = 0;
        foreach (string line in lines)
        {
            foreach (char c in line)
            {
                if (pixelIndex >= pixels.Length) break;

                int colourIndex = Convert.ToInt32(c.ToString(), 16);

                if (colourIndex >= 0 && colourIndex < Constants.Colors.Length)
                {
                    pixels[pixelIndex] = Constants.Colors[colourIndex];
                }
                else
                {
                    pixels[pixelIndex] = Color.Blank;
                }

                pixelIndex++;
            }
        }

        // AAAAAAHHHHHH garbage 
        // forces memory address to not change
        GCHandle handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            Image img = new Image
            {
                Data = (void*)handle.AddrOfPinnedObject(),
                Width = width,
                Height = height,
                Mipmaps = 1,
                Format = PixelFormat.UncompressedR8G8B8A8
            };

            Texture2D texture = Raylib.LoadTextureFromImage(img);
            Raylib.SetTextureFilter(texture, TextureFilter.Point);
            return texture;
        }
        finally
        {
            handle.Free();
        }
    }
    
}