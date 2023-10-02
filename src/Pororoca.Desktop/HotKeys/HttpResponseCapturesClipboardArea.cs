using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Desktop.HotKeys;

public sealed class HttpResponseCapturesClipboardArea : SimpleClipboardArea<PororocaHttpResponseValueCapture>
{
    internal static readonly HttpResponseCapturesClipboardArea Instance = new();

    public override List<PororocaHttpResponseValueCapture> FetchCopies() =>
        this.copied.Select(o => o.Copy())
                   .ToList();
}