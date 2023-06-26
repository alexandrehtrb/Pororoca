using System.Text;
using System.Text.Json;

namespace Pororoca.Domain.Features.Common;

public static class JsonUtils
{
    public static bool IsValidJson(string txt, JsonReaderOptions options = default)
    {
        byte[] utf8bytes = Encoding.UTF8.GetBytes(txt);
        Utf8JsonReader reader = new(utf8bytes, options);
        try
        {
            reader.Read();
        }
        catch
        {
            return false;
        }
        try
        {
            reader.TrySkip();
        }
        catch
        {
            return false;
        }
        return true;
    }
}