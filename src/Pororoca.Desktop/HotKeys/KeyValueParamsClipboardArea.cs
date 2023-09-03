using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.HotKeys;

public sealed class KeyValueParamsClipboardArea : SimpleClipboardArea<PororocaKeyValueParam>
{
    internal static readonly KeyValueParamsClipboardArea Instance = new();

    public override List<PororocaKeyValueParam> FetchCopies() =>
        this.copied.Select(o => o.Copy())
                   .ToList();
}