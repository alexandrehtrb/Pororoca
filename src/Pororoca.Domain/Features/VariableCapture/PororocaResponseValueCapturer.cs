using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml;

namespace Pororoca.Domain.Features.VariableCapture;

public static partial class PororocaResponseValueCapturer
{
    private static readonly Regex arrayElementRegex = GenerateArrayElementRegex();
    private static readonly Regex defaultXmlNamespaceRegex = GenerateDefaultXmlNamespaceRegex();
    private static readonly Regex xmlNamespacesRegex = GenerateXmlNamespacesRegex();

    [GeneratedRegex("\\[(\\d+)\\]")]
    private static partial Regex GenerateArrayElementRegex();

    [GeneratedRegex("xmlns=\"(?<Url>[\\w\\d:\\/\\.\\-_]+)\"")]
    private static partial Regex GenerateDefaultXmlNamespaceRegex();
    
    [GeneratedRegex("xmlns:(?<Prefix>\\w+)=\"(?<Url>[\\w\\d:\\/\\.\\-_]+)\"")]
    private static partial Regex GenerateXmlNamespacesRegex();

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

    internal static (XmlDocument, XmlNamespaceManager)? LoadXmlDocumentAndNamespaceManager(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return null;

        try
        {
            XmlDocument doc = new();
            doc.LoadXml(xml);
            XmlNamespaceManager nsm = new(doc.NameTable);
            var namespaces = GetXmlNamespaces(xml);
            foreach ((string prefix, string url) in namespaces)
            {
                nsm.AddNamespace(prefix, url);
            }
            return (doc, nsm);
        }
        catch
        {
            return null;
        }
    }

    private static (string Prefix, string Url)[] GetXmlNamespaces(string xml)
    {
        var matches = xmlNamespacesRegex.Matches(xml);
        var list = matches.Select(m =>
        {
            string prefix = m.Groups["Prefix"].Value;
            string url = m.Groups["Url"].Value;
            return (prefix, url);
        }).Distinct().ToList();

        list.AddRange(defaultXmlNamespaceRegex.Matches(xml).Select(m =>
        {
            string prefix = string.Empty;
            string url = m.Groups["Url"].Value;
            return (prefix, url);
        }).Distinct());

        return list.ToArray();
    }

    public static string? CaptureJsonValue(string path, string json)
    {
        try
        {
            string[] subpaths = path.Split('.');
            var jsonNode = JsonNode.Parse(json);
            
            foreach (string subpath in subpaths)
            {
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
}