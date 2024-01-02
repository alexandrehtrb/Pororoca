using System.Collections.Frozen;
using System.Text;
using System.Text.RegularExpressions;
using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Domain.Features.ResponseParsing;

internal static partial class PororocaHttpMultipartResponseBodyReader
{
    private static readonly Regex multipartBoundaryRegex = GenerateMultipartBoundaryRegex();

    [GeneratedRegex("boundary=\"(?<boundary>[\\w\\d\\-]+)\"")]
    private static partial Regex GenerateMultipartBoundaryRegex();

    internal static List<byte[]> Split(this ReadOnlySpan<byte> input, ReadOnlySpan<byte> splitterSequence)
    {
        List<byte[]> parts = new();
        var scanning = input;
        int splitterOffset = splitterSequence.Length;
        while (true)
        {
            int end = scanning.IndexOf(splitterSequence);
            if (end == -1)
            {
                if (scanning.Length > 0)
                {
                    parts.Add(scanning.ToArray());
                }
                break;
            }
            else
            {
                if (end > 0)
                {
                    parts.Add(scanning[..end].ToArray());
                }
                scanning = scanning[(end + splitterOffset)..];
            }
        }
        return parts;
    }

    internal static string ReadMultipartBoundary(string contentType)
    {
        try
        {
            var match = multipartBoundaryRegex.Match(contentType);
            string? boundary = match.Groups["boundary"]?.Value;
            return string.IsNullOrWhiteSpace(boundary) ? string.Empty : ("--" + boundary);
        }
        catch
        {
            return string.Empty;
        }
    }

    internal static PororocaHttpResponseMultipartPart[]? ReadMultipartResponseParts(ReadOnlySpan<byte> binaryBody, string boundary)
    {
        try
        {
            List<PororocaHttpResponseMultipartPart> parts = new();
            var binaryParts = binaryBody.TrimEnd("--\r\n"u8).Split(Encoding.UTF8.GetBytes(boundary));
            foreach (ReadOnlySpan<byte> binaryPart in binaryParts)
            {
                var headersAndBody = binaryPart.Split("\r\n\r\n"u8);
                byte[] headers = headersAndBody[0];
                byte[] body = headersAndBody.Count > 1 ? headersAndBody[1] : [];
                var partHeaders = ParseBinaryHeaders(headers);
                var partBody = body.AsSpan().TrimEnd("\r\n"u8);
                parts.Add(new(partHeaders, partBody.ToArray()));
            }
            return parts.ToArray();
        }
        catch
        {
            return null;
        }
    }

    private static FrozenDictionary<string, string> ParseBinaryHeaders(byte[] binaryHeaders)
    {
        Dictionary<string, string> headers = new();
        var headersLines = new ReadOnlySpan<byte>(binaryHeaders).Split("\r\n"u8);
        foreach (byte[] headerLine in headersLines)
        {
            string[] parts = Encoding.UTF8.GetString(headerLine).Split(": ");
            headers.Add(parts[0], parts[1]);
        }
        return headers.ToFrozenDictionary();
    }
}