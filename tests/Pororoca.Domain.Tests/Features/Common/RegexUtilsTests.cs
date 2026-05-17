using System.Text.RegularExpressions;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.VariableResolution;
using Xunit;
using static Pororoca.Domain.Tests.Features.Common.TestRegexDefiner;

namespace Pororoca.Domain.Tests.Features.Common;

public static class RegexUtilsTests
{
    #region DELIMIT TEXT PARTS OVER REGEXES

    // 00aaaa0aaaa0000a0
    // aa0a
    // <a><b>TEXT<b/><c>12345<c/><a/>
    // {{ BaseUrl}}/api/get?user={{$randomInt}}&name={{ UserName }}
    // https://{{ BaseUrl}}/api/get?user={{$randomInt}}&name={{ UserName }}
    public static IEnumerable<object[]> GetTestDataForDelimitWithRegexes()
    {
        yield return new object[]
        {
            "00aaaa0aaaa0000a0",
            new[] { LetterBlock },
            new[] { (null, 0, 2), (LetterBlock, 2, 4), (null, 6, 1), (LetterBlock, 7, 4), (null, 11, 4), (LetterBlock, 15, 1), (null, 16, 1) }

    };
        yield return new object[]
        {
            "00aaaa0aaaa0000a0",
            new[] { NumberBlock },
            new[] { (NumberBlock,0,2), (null,2,4), (NumberBlock,6,1), (null,7,4), (NumberBlock,11,4), (null,15,1), (NumberBlock,16,1) }
        };
        yield return new object[]
        {
            "00aaaa0aaaa0000a0",
            new[] { LetterBlock,NumberBlock },
            new[] { (NumberBlock,0,2), (LetterBlock, 2,4), (NumberBlock,6,1), (LetterBlock, 7,4), (NumberBlock,11,4), (LetterBlock, 15,1), (NumberBlock,16,1) }
        };
        yield return new object[]
        {
            "aa0a",
            new[] { LetterBlock,NumberBlock },
            new[] { (LetterBlock, 0,2), (NumberBlock, 2,1), (LetterBlock,3,1) }
        };
        yield return new object[]
        {
            "aa0a",
            new[] { LetterBlock,NumberSingle },
            new[] { (LetterBlock, 0,2), (NumberSingle, 2,1), (LetterBlock,3,1) }
        };
        yield return new object[]
        {
            "aa0a",
            new[] { LetterSingle, NumberBlock },
            new[] { (LetterSingle, 0,1), (LetterSingle, 1, 1), (NumberBlock, 2,1), (LetterSingle, 3,1) }
        };
        yield return new object[]
        {
            "aa0a",
            new[] { LetterSingle, NumberSingle },
            new[] { (LetterSingle, 0,1), (LetterSingle, 1, 1), (NumberSingle, 2,1), (LetterSingle, 3,1) }
        };
        yield return new object[]
        {
            "<a><b>TEXT<b/><c>12345<c/><a/>",
            new[] { XmlTag },
            new[] { (XmlTag, 0,3), (XmlTag, 3, 3), (null,6,4), (XmlTag, 10, 4), (XmlTag, 14, 3), (null,17,5), (XmlTag, 22,4), (XmlTag, 26, 4)}
        };
        yield return new object[]
        {
            "{{ BaseUrl}}/api/get?user={{$randomInt}}&name={{ UserName }}",
            new[] { PororocaVar },
            new[] { (PororocaVar, 0,12), (null, 12,14), (PororocaVar, 26,14), (null,40,6),(PororocaVar, 46,14) }
        };
        yield return new object[]
        {
            "https://{{ BaseUrl}}/api/get?user={{$randomInt}}&name={{ UserName }}",
            new[] { PororocaVar },
            new[] { (null,0,8), (PororocaVar, 8,12), (null, 20,14), (PororocaVar, 34,14), (null,48,6),(PororocaVar, 54,14) }
        };
    }

    [Theory]
    [MemberData(nameof(GetTestDataForDelimitWithRegexes))]
    public static void Should_delimit_text_parts_with_regexes_correctly(string input, TestRegexDefiner[] regexesDefs, (TestRegexDefiner? RegexDefinition, int Start, int Length)[] expectedParts)
    {
        // GIVEN, WHEN
        var parts = RegexUtils.DelimitTextPartsOverRegexes(regexesDefs, input)
                              .Select(x => (x.Pattern, x.Start, x.Length))
                              .ToArray();

        // THEN
        Assert.NotNull(parts);
        Assert.Equal(expectedParts, parts);
    }

    #endregion
}

public partial class TestRegexDefiner : IRegexDefiner
{
    internal static readonly TestRegexDefiner NumberBlock = new(new("\\d+"));
    internal static readonly TestRegexDefiner NumberSingle = new(new("\\d"));
    internal static readonly TestRegexDefiner LetterBlock = new(new("[A-Za-z_]+"));
    internal static readonly TestRegexDefiner LetterSingle = new(new("[A-Za-z_]"));
    internal static readonly TestRegexDefiner XmlTag = new(new("\\<\\w+\\/?>"));
    internal static readonly TestRegexDefiner PororocaVar = new(IPororocaVariableResolver.PororocaVariableRegex);

    public Regex Pattern { get; }

    private TestRegexDefiner(Regex pattern) =>
        Pattern = pattern;
}