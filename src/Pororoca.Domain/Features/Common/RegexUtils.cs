using System.Text.RegularExpressions;

namespace Pororoca.Domain.Features.Common;

public interface IRegexDefiner
{
    public Regex Pattern { get; }
}

public static class RegexUtils
{
    // Este método só é usado no Pororoca.Desktop,
    // porém está na camada de Domain porque é lógica pesada
    // e queremos testes unitários nele.
    public static List<(T? Pattern, int Start, int Length, Match? Match)> DelimitTextPartsOverRegexes<T>(IEnumerable<T> patternDefinitions, string? input)
        where T : class, IRegexDefiner
    {
        if (String.IsNullOrEmpty(input))
        {
            return [];
        }

        var list = patternDefinitions.SelectMany(d =>
        {
            var matches = d.Pattern.Matches(input);
            return matches.Select(m => ((T?)d, m.Index, m.Length, (Match?)m));
        }).OrderBy(x => x.Index).ToList();


        if (list.Count == 0)
        {
            return [(null, 0, input.Length, null)];
        }
        else
        {
            // First match begins after the start of the string.
            // Let's mark this part with null.
            if (list[0].Index > 0)
            {
                list.Insert(0, (null, 0, list[0].Index, null));
            }

            for (int i = 0; i < list.Count - 1; i++)
            {
                var (_, currentStart, currentLength, _) = list[i];
                var (_, nextStart, _, _) = list[i + 1];

                // This means that there is some space between 
                // the current match and the next match.
                // Let's mark this part with null.
                int currentToNextLength = nextStart - (currentStart + currentLength);
                if (currentToNextLength > 0)
                {
                    list.Insert(i + 1, (null, currentStart + currentLength, currentToNextLength, null));
                    i++; // no need to check the newly created part again
                }
            }

            // Last match ends before the end of the string.
            // Let's mark this part with null.
            int endOfLastMatch = list[^1].Index + list[^1].Length;
            if (endOfLastMatch < input.Length)
            {
                list.Add((null, endOfLastMatch, input.Length - endOfLastMatch, null));
            }

            return list;
        }
    }
}