using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml;

namespace Pororoca.Domain.Features.VariableCapture;

public static partial class PororocaResponseValueCapturer
{
    private static readonly Regex arrayElementRegex = GenerateArrayElementRegex();
    private static readonly Regex firstWhereRegex = FirstWhereRegex();

    [GeneratedRegex("\\[(\\d+)\\]")]
    private static partial Regex GenerateArrayElementRegex();
    [GeneratedRegex("firstWhere\\((.+)\\)")]
    private static partial Regex FirstWhereRegex();

    public static string? CaptureXmlValue(string xpath, XmlDocument? doc, XmlNamespaceManager? nsm)
    {
        if (doc is null || nsm is null)
        {
            return null;
        }

        try
        {
            var node = doc.SelectSingleNode(xpath, nsm);
            return node?.InnerText;
        }
        catch
        {
            return null;
        }
    }

    public static string? CaptureJsonValue(string path, string json)
    {
        try
        {
            string[] subpaths = path.Split('.');
            var jsonNode = JsonNode.Parse(json);

            foreach (string subpath in subpaths)
            {
                if (IsFunction(subpath, jsonNode, out var functionJsonNode))
                {
                    jsonNode = functionJsonNode;
                    continue;
                }

                if (IsArrayElementSubpath(subpath, out string? elementName, out int? index1, out int? index2))
                {
                    if (elementName?.Equals("$", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        jsonNode = jsonNode?.AsArray()[(int)index1!];
                    }
                    else
                    {
                        jsonNode = jsonNode?[elementName!]?.AsArray()[(int)index1!];
                    }

                    if (index2 is int i2)
                    {
                        jsonNode = jsonNode?.AsArray()[i2];
                    }
                }
                else if (subpath.Equals("$", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                else
                {
                    jsonNode = jsonNode?[subpath];
                }
            }

            if (jsonNode is JsonValue jv)
            {
                return jv.ToString();
            }
            else
            {
                return jsonNode?.ToJsonString();
            }
        }
        catch
        {
            return null;
        }
    }

    private static bool IsArrayElementSubpath(string subpath, out string? elementName, out int? index1, out int? index2)
    {
        var matches = arrayElementRegex.Matches(subpath);
        string[] captures = matches.Select(m => m.Value[1..^1]).ToArray();
        if (captures.Length == 1)
        {
            elementName = subpath[0..(subpath.IndexOf('['))];
            index1 = int.Parse(captures[0]);
            index2 = null;
            return true;
        }
        else if (captures.Length == 2)
        {
            elementName = subpath[0..(subpath.IndexOf('['))];
            index1 = int.Parse(captures[0]);
            index2 = int.Parse(captures[1]);
            return true;
        }
        else
        {
            elementName = null;
            index1 = index2 = null;
            return false;
        }
    }

    private static bool IsFunction(string subpath, JsonNode? jsonNode, out JsonNode? result)
    {
        if (string.IsNullOrWhiteSpace(subpath) || jsonNode is null)
        {
            result = null;
            return false;
        }

        var match = firstWhereRegex.Match(subpath);
        var condition = match?.Groups?.Values?.Last().Value;
        if (match != null && match.Success)
            EvaluateFirstWhereCondition(condition, jsonNode);

        switch (subpath)
        {
            case "count()":
                result = GetCount(jsonNode);
                return true;
            case "first()":
                result = GetFirstArrayElement(jsonNode);
                return true;
            case "last()":
                result = GetLastArrayElement(jsonNode);
                return true;

            default:
                result = null;
                return false;
        }
    }

    private static JsonNode GetCount(JsonNode node)
    {
        int count = node.AsArray().Count;
        return count;
    }

    private static JsonNode? GetFirstArrayElement(JsonNode node)
    {
        var firstElement = node.AsArray().First();
        return firstElement;
    }

    private static JsonNode? GetLastArrayElement(JsonNode node)
    {
        var lastElement = node.AsArray().Last();
        return lastElement;
    }

    private static JsonNode? EvaluateFirstWhereCondition(string? condition, JsonNode node)
    {

        return null;
    }
}