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

    public unsafe static Sound[] LoadSoundEffects(string sfxText)
    {
        Sound[] sfxList = new Sound[64];
        if (string.IsNullOrWhiteSpace(sfxText)) return sfxList;

        string[] lines = sfxText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("--") || string.IsNullOrEmpty(trimmed)) continue;

            string[] mainParts = trimmed.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (mainParts.Length >= 5)
            {
                if (int.TryParse(mainParts[0], out int id) && id >= 0 && id < 64)
                {
                    int speed = int.Parse(mainParts[1]);

                    List<SFXStep> steps = new List<SFXStep>();
                    for (int i = 4; i < mainParts.Length && i < 36 + 4; i++)
                    {
                        string[] stepParts = mainParts[i].Split(':');
                        if (stepParts.Length >= 4)
                        {
                            steps.Add(new SFXStep
                            {
                                Pitch = int.Parse(stepParts[0]),
                                Waveform = int.Parse(stepParts[1]),
                                Volume = int.Parse(stepParts[2]),
                                Effect = int.Parse(stepParts[3]),
                            });
                        }
                    }

                    sfxList[id] = GenerateProceduralSound(speed, steps);
                }
            }
        }

        return sfxList;
    }

    public struct SFXStep
    {
        public int Pitch;
        public int Waveform;
        public int Volume;
        public int Effect;
    }
    
    private unsafe static Sound GenerateProceduralSound(int speed, List<SFXStep> steps)
    {
        if (steps.Count == 0) return new Sound();
        
        // move to lower sample rate for retro sound
        // more than enough i dont need better
        int sampleRate = 22050;
        int samplesPerStep = (int)(sampleRate * (speed / 60.0f)); // scale length based on framerate
        int totalSamples = steps.Count * samplesPerStep;
        short[] samples = new short[totalSamples];

        int globalIndex = 0;
        float currentPhase = 0f;

        foreach (var step in steps)
        {
            if (step.Volume == 0)
            {
                for (int i = 0; i < samplesPerStep; i++)
                {
                    samples[globalIndex++] = 0;
                }
                continue;
            }

            // convert pitch (0-63) to frequency in hertz, so 36 is C-2 / ~130.81Hz
            float targetFreq = 440.0f * (float)Math.Pow(2.0, (step.Pitch - 33) / 12.0);
            float amplitude = (step.Volume / 7.0f) * 0.4f;

            for (int i = 0; i < samplesPerStep; i++)
            {
                float t = (float)i / samplesPerStep;
                float freq = targetFreq;

                // handle effects
                if (step.Effect == 1) // slide
                {
                    freq = targetFreq * (0.8f + (0.4f * t));
                }
                else if (step.Effect == 2) // vibrato
                {
                    freq += (float)Math.Sin(t * 30.0f) * 4.0f;
                }
                else if (step.Effect == 5) // fade out envelope
                {
                    amplitude *= (1.0f - t);
                }

                currentPhase += freq / sampleRate;
                currentPhase %= 1.0f;

                float sample = step.Waveform switch
                {
                    0 => (float)Math.Sin(currentPhase * 2.0 * Math.PI),
                    1 => currentPhase < 0.5f ? 1.0f : -1.0f,
                    2 => (currentPhase * 2.0f) - 1.0f,
                    _ => ((float)Random.Shared.NextDouble() * 2.0f) - 1.0f
                };

                // per step click suppression envelope
                float clickEnvelope = Math.Min(1.0f, Math.Min(i / 50.0f, (samplesPerStep - i) / 50.0f));

                samples[globalIndex] = (short)(sample * clickEnvelope * amplitude * 32767);
                globalIndex++;
            }
        }
        
        fixed (short* ptr = samples)
        {
            Wave wave = new Wave
            {
                Data = (void*)ptr,
                FrameCount = (uint)totalSamples,
                SampleRate = (uint)sampleRate,
                Channels = 1, // mono sound
                SampleSize = 16, // 16 bit
            };

            Sound sound = Raylib.LoadSoundFromWave(wave);
            return sound;
        }
    }
    
}