using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Desktop.HotKeys;

public sealed class FormDataParamsClipboardArea : SimpleClipboardArea<PororocaHttpRequestFormDataParam>
{
    internal static readonly FormDataParamsClipboardArea Instance = new();

    public override List<PororocaHttpRequestFormDataParam> FetchCopies() =>
        this.copied.Select(o => o.Copy())
                   .ToList();
}