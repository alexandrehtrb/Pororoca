using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.HotKeys;

public sealed class VariablesClipboardArea : SimpleClipboardArea<PororocaVariable>
{
    internal static readonly VariablesClipboardArea Instance = new();

    public override List<PororocaVariable> FetchCopies() =>
        this.copied.Select(o => o.Copy())
                   .ToList();
}