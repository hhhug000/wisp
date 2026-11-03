using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
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
                if (char.IsWhiteSpace(c)) continue;
                
                if (pixelIndex >= pixels.Length) break;

                if (c == '.')
                {
                    pixels[pixelIndex] = Color.Blank;
                }
                else
                {

                    int colourIndex = Convert.ToInt32(c.ToString(), 16);

                    if (colourIndex >= 0 && colourIndex < Constants.Colors.Length)
                    {
                        pixels[pixelIndex] = Constants.Colors[colourIndex];
                    }
                    else
                    {
                        pixels[pixelIndex] = Color.Blank;
                    }
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

    public static int[,] ParseMap(string mapText)
    {
        // split maptext into lines
        string[] lines = mapText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        int rows = lines.Length;
        int cols = rows > 0 ? lines[0].Length / 2 : 0;

        int[,] mapGrid = new int[cols, rows];

        // loop through all the rows
        for (int y = 0; y < rows; y++)
        {
            string line = lines[y];
            for (int x = 0; x < cols; x++)
            {
                // 2 hex chars so it needs to be *2
                int charIndex = x * 2;
                if (charIndex + 1 < line.Length)
                {
                    string hexPair = line.Substring(charIndex, 2);
                    if (int.TryParse(hexPair, System.Globalization.NumberStyles.HexNumber, null, out int tileId))
                    {
                        mapGrid[x, y] = tileId;
                    }
                    else
                    {
                        mapGrid[x, y] = 0;
                    }
                }
            }
        }

        return mapGrid;
    }

    public unsafe static Sound[] LoadSoundEffect(string sfxText)
    {
        Sound[] sfxList = new Sound[64];

        if (string.IsNullOrWhiteSpace(sfxText))
        {
            return sfxList;
        }

        string[] lines = sfxText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            // the format for it should be like this:
            // id, starting frequency, ending frequency, waveform, duration
            string[] parts = trimmed.Split(',');
            if (parts.Length >= 5)
            {
                if (int.TryParse(parts[0].Trim(), out int id) && id >= 0 && id < sfxList.Length)
                {
                    float startFreq = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                    float endFreq = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
                    int waveform = int.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
                    int durationFrames = int.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);

                    sfxList[id] = GenerateProceduralSound(startFreq, endFreq, waveform, durationFrames);
                }
            }
        }

        return sfxList;
    }

    private unsafe static Sound GenerateProceduralSound(float startFreq, float endFreq, int waveform, int durationFrames)
    {
        // this is cd quality
        // more than enough i dont need better
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * (durationFrames / 60.0f)); // scale length based on framerate
        short[] samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            
            float currentFreq = startFreq + (endFreq - startFreq) * t;
            
            float sampleNormalized = 0f;
            float angle = 2.0f * (float)Math.PI * currentFreq * ((float)i / sampleRate);
            
            if (waveform == 0) // sine wave
            {
                sampleNormalized = (float)Math.Sin(angle);
            }
            else if (waveform == 1) // square wave
            {
                sampleNormalized = Math.Sin(angle) >= 0 ? 1.0f : -1.0f;
            }
            else // noise
            {
                sampleNormalized = ((float)Random.Shared.NextDouble() * 2.0f) - 1.0f;
            }

            // volume envelope
            float envelope = 1.0f - t;

            // +-32768 is 16 bit PCM
            // also half volume
            samples[i] = (short)(sampleNormalized * envelope * 32767 * 0.5f);
        }
        
        fixed (short* ptr = samples)
        {
            Wave wave = new Wave
            {
                Data = ptr,
                SampleCount = (uint)sampleCount,
                SampleRate = (uint)sampleRate,
                Channels = 1, // mono sound
                SampleSize = 16, // 16 bit
                Type = PixelFormat.UncompressedGrayscale
            };

            Sound sound = Raylib.LoadSoundFromWave(wave);
            return sound;
        }
    }
    
}