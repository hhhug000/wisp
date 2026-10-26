namespace wisp;

public class FileParser
{
    // function to split a wisp file into the individual sections
    public static Dictionary<String, List<string>> SplitToSections(string rawText)
    {
        // split the raw text into lines to be separated
        string[] lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        if (lines.Length < 2)
        {
            throw new System.IO.InvalidDataException("Invalid cartridge (Too short to contain header)");
        }

        string header = lines[0].Trim();
        if (!header.Equals("wisp cartridge", StringComparison.OrdinalIgnoreCase))
        {
            throw new System.IO.InvalidDataException($"Invalid cartridge (Expected 'wisp cartridge', got '{header}')");
        }

        string versionLine = lines[1].Trim();
        if (!versionLine.StartsWith("version ", StringComparison.OrdinalIgnoreCase))
        {
            throw new System.IO.InvalidDataException($"Invalid cartridge (Expected 'version' header, got '{versionLine}')");
        }
        
        var sections = new Dictionary<string, List<string>>();
        string currentSection = string.Empty;

        for (int i = 2; i < lines.Length; i++)
        {
            string rawLine = lines[i];
            string trimmed = rawLine.Trim();

            if (trimmed.StartsWith("__") && trimmed.EndsWith("__") && trimmed.Length > 4)
            {
                currentSection = trimmed.Substring(2, trimmed.Length - 4).ToLowerInvariant();

                if (!sections.ContainsKey(currentSection))
                {
                    sections[currentSection] = new List<string>();
                }

                continue;
            }

            if (!string.IsNullOrEmpty(currentSection))
            {
                sections[currentSection].Add(rawLine);
            }
        }
        
        return sections;
    }
}