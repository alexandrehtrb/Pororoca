using Pororoca.Desktop.ViewModels;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.HotKeys;

public sealed class VariablesClipboardArea : ViewModelBase
{
    internal static readonly VariablesClipboardArea Instance = new();

    private readonly List<PororocaVariable> copiedVars = new();

    [Reactive]
    public bool CanPasteVariables { get; private set; }

    public void ClearPushedVariables()
    {
        this.copiedVars.Clear();
        CanPasteVariables = false;
    }

    public void PushToArea(params PororocaVariable[] varsToCopy)
    {
        this.copiedVars.Clear();
        this.copiedVars.AddRange(varsToCopy);
        CanPasteVariables = this.copiedVars.Any();
    }

    public List<PororocaVariable> FetchCopiesOfVariables() =>
        this.copiedVars.Select(o => o.Copy())
                       .ToList();
}