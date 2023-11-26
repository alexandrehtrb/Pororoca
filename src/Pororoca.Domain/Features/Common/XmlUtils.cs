using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Pororoca.Domain.Features.Common;

internal static partial class XmlUtils
{
    private static readonly Regex defaultXmlNamespaceRegex = GenerateDefaultXmlNamespaceRegex();
    private static readonly Regex xmlNamespacesRegex = GenerateXmlNamespacesRegex();

    [GeneratedRegex("xmlns=\"(?<Url>[\\w\\d:\\/\\.\\-_]+)\"")]
    private static partial Regex GenerateDefaultXmlNamespaceRegex();
    
    [GeneratedRegex("xmlns:(?<Prefix>\\w+)=\"(?<Url>[\\w\\d:\\/\\.\\-_]+)\"")]
    private static partial Regex GenerateXmlNamespacesRegex();

    internal static void LoadXmlDocumentAndNamespaceManager(string xml, out XmlDocument? doc, out XmlNamespaceManager? nsm)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            doc = null;
            nsm = null;
            return;
        }

        try
        {
            doc = new();
            doc.LoadXml(xml);
            nsm = new(doc.NameTable);
            var namespaces = GetXmlNamespaces(xml);
            foreach ((string prefix, string url) in namespaces)
            {
                nsm.AddNamespace(prefix, url);
            }
        }
        catch
        {
            doc = null;
            nsm = null;
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

    internal static string PrettifyXml(string xml)
    {
        string result = xml;

        using MemoryStream mStream = new();
        using XmlTextWriter writer = new(mStream, Encoding.UTF8);
        XmlDocument document = new();

        try
        {
            // Load the XmlDocument with the XML.
            document.LoadXml(xml);

            writer.Formatting = Formatting.Indented;

            // Write the XML into a formatting XmlTextWriter
            document.WriteContentTo(writer);
            writer.Flush();
            mStream.Flush();

            // Have to rewind the MemoryStream in order to read
            // its contents.
            mStream.Position = 0;

            // Read MemoryStream contents into a StreamReader.
            using StreamReader sReader = new(mStream);

            // Extract the text from the StreamReader.
            string formattedXml = sReader.ReadToEnd();

            result = formattedXml;
        }
        catch (XmlException)
        {
            // Handle the exception
        }

        mStream.Close();
        writer.Close();

        return result;
    }
}