using System.Text.RegularExpressions;

namespace Pororoca.Domain.Features.ImportRequest;

public static partial class CurlRequestImporter
{
    private static readonly Regex CmdMultiLineRegex = GenerateCmdMultiLineRegex();

    private static readonly Regex CmdLineKvpsRegex = GenerateCmdLineKvpsRegex();

    private static readonly Regex CmdLineUrlRegex = GenerateCmdLineUrlRegex();

    // \ -> line-break for Unix
    // ` -> line-break for PowerShell
    [GeneratedRegex("\\s+[\\\\`][\\r\\s]*\\n")]
    private static partial Regex GenerateCmdMultiLineRegex();

    [GeneratedRegex("\\s(?<key>--?[\\w\\d\\.\\-/]+)")]
    private static partial Regex GenerateCmdLineKvpsRegex();

    [GeneratedRegex("(?<url>https?://[\\w\\d\\.\\,\\-/:%$@!_&?=#~]+)")]
    private static partial Regex GenerateCmdLineUrlRegex();

    internal static List<KeyValuePair<string, string>> ParseCurlCommandLineParams(string cmdLine)
    {
        string singleLined = CmdMultiLineRegex.Replace(cmdLine, " ");
        ReadOnlySpan<char> singleLinedAsSpan = singleLined.AsSpan();

        var mc = CmdLineKvpsRegex.Matches(singleLined);
        List<KeyValuePair<string, string>> kvps = new(mc.Count);
        foreach (Match match in mc)
        {
            string key = match.ValueSpan.Trim().ToString();
            int valueStartIndex = match.Index + match.Length;
            int lastSpaces = singleLinedAsSpan.Slice(valueStartIndex).IndexOfAnyExcept(' ');
            if (lastSpaces > 0)
            {
                valueStartIndex += lastSpaces;
            }
            ReadOnlySpan<char> value = ParseValueOfCmdLineArgument(singleLinedAsSpan, key, valueStartIndex, out int valueEndIndex);
            string? valueStr;
            // after reading the part to remove, we can now clean and unescape strings
            if (value != Span<char>.Empty && value.Length >= 2 && value[0] == '\'' && value[^1] == '\'')
            {
                valueStr = value[1..^1].ToString().Replace("\\'", "'");
            }
            else if (value != Span<char>.Empty && value.Length >= 2 && value[0] == '"' && value[^1] == '"')
            {
                valueStr = value[1..^1].ToString().Replace("\\\"", "\"");
            }
            else
            {
                valueStr = value.ToString();
            }

            kvps.Add(new(key, valueStr));
        }

        if (!kvps.Any(kv => kv.Key == "--url"))
        {
            // if not specified via flag --url,
            // we'll have to read the URL via regex

            var urlMatch = CmdLineUrlRegex.Match(cmdLine);
            if (urlMatch != null)
            {
                kvps.Add(new("--url", urlMatch.Value));
            }
        }

        return kvps;
    }

    private static ReadOnlySpan<char> ParseValueOfCmdLineArgument(ReadOnlySpan<char> singleLinedCmd, string key, int startIndex, out int endIndex)
    {
        if (startIndex >= singleLinedCmd.Length)
        {
            // it's just a flag at the end of the cmd line. no value.
            // e.g. --http2
            endIndex = startIndex;
            return null;
        }

        char startingChar = singleLinedCmd[startIndex];

        if (startingChar == '-')
        {
            // it's just a flag in the middle of the cmd line. no value.
            // after it, there's another flag.
            // e.g. --http2
            endIndex = startIndex;
            return null;
        }

        if (key != "--url" && key != "--referer" && key != "-e")
        {
            int nextSliceSize;
            for (nextSliceSize = 9; nextSliceSize >= 6; nextSliceSize--)
            {
                if ((startIndex + nextSliceSize) < singleLinedCmd.Length)
                {
                    break;
                }
            }
            if (nextSliceSize >= 6)
            {
                string nextTextSlice = singleLinedCmd.Slice(startIndex, nextSliceSize).ToString();
                if (nextTextSlice.StartsWith("https://")
                 || nextTextSlice.StartsWith("\"https://")
                 || nextTextSlice.StartsWith("'https://")
                 || nextTextSlice.StartsWith("http://")
                 || nextTextSlice.StartsWith("\"http://")
                 || nextTextSlice.StartsWith("'http://"))
                {
                    // this is the case of a valueless flag 
                    // before an URL that is placed at the end of the cmd.
                    endIndex = startIndex;
                    return null;
                }
            }
        }

        if (char.IsAsciiLetterOrDigit(startingChar) || startingChar == '$' || startingChar == '.' || startingChar == '/' || startingChar == '=' || startingChar == '@')
        {
            // raw unescaped string. look for next whitespace or end of string.
            // e.g. --request POST, -X PUT, -O $url
            endIndex = singleLinedCmd.Slice(startIndex).IndexOf(' ');
            if (endIndex == -1)
            {
                return singleLinedCmd[startIndex..]; // terminates at end of line
            }
            else
            {
                // this is because the IndexOf() above
                // counts from the beginning,
                // but the Slice() above resets beginning to zero.
                endIndex += startIndex;
                return singleLinedCmd[startIndex..endIndex]; // terminates before end of line
            }
        }

        char delimiter = startingChar;
        int currentStartIndex = startIndex;
        int currentEndIndex;
        endIndex = startIndex;
        if (delimiter == '\'' || delimiter == '"')
        {
searchClosure:
            currentEndIndex = singleLinedCmd.Slice(currentStartIndex + 1).IndexOf(delimiter);
            if (currentEndIndex == -1)
            {
                // closure delimiter not found; malformatted
                return null;
            }

            endIndex += currentEndIndex + 1;

            char previousChar = singleLinedCmd[endIndex - 1];
            if (previousChar == '\\')
            {
                // it's an inner quote, e.g. "{\"a\": 1}"
                // it's not the ending delimiter                
                currentStartIndex = endIndex;
                goto searchClosure;
            }
            else
            {
                // found closure delimiter
                return singleLinedCmd[startIndex..(endIndex + 1)];
            }
        }

        return null;
    }
}
