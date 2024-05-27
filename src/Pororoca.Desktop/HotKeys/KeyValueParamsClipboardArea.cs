using Avalonia.Input.Platform;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.HotKeys;

public sealed class KeyValueParamsClipboardArea : SimpleClipboardArea<PororocaKeyValueParam>
{
    internal static readonly KeyValueParamsClipboardArea Instance = new();

    public override bool CanPaste => true;

    public override List<PororocaKeyValueParam> FetchCopies() =>
        this.copied.Select(o => o.Copy())
                   .ToList();

    internal static async Task<IEnumerable<PororocaKeyValueParam>> GetColonSeparatedValuesFromSystemClipboardAreaAsync()
    {
        if (MainWindow.Instance!.Clipboard is IClipboard systemClipboard)
        {
            try
            {
                string? systemClipboardAreaText = await systemClipboard.GetTextAsync();
                if (systemClipboardAreaText is null) return [];
                return systemClipboardAreaText
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(line =>
                    {
                        int i = line.IndexOf(':');
                        string headerName = line[..i].Trim(), headerValue = line[(i+1)..].Trim();
                        return new PororocaKeyValueParam(true, headerName, headerValue);
                    });
            }
            catch { return []; }
        }
        else
        {
            return [];
        }
    }
}