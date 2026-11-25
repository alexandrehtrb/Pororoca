using ReactiveUI.Primitives;
using Pororoca.Desktop.Converters;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Pororoca.Desktop.ViewModels;

public sealed partial class EditableTextBlockViewModel : ViewModelBase
{
    private readonly Action<string> onNameUpdated;

    [Reactive]
    public partial bool HasIcon { get; private set; }

    public EditableTextBlockIcon? Icon
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            HasIcon = value is not null;
        }
    }

    [Reactive]
    public partial string Txt { get; set; }

    public bool IsEditing
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            OnIsEditingChanged?.Invoke(value);
        }
    }

    public Action<bool>? OnIsEditingChanged { get; set; }

    public ReactiveCommand<RxVoid, RxVoid> EditOrApplyTxtChangeCmd { get; }

    public EditableTextBlockViewModel(string name, Action<string> onNameUpdated)
    {
        this.onNameUpdated = onNameUpdated;
        Txt = name;
        IsEditing = false;
        EditOrApplyTxtChangeCmd = ReactiveCommand.Create(EditOrApplyTxtChange);
    }

    public void EditOrApplyTxtChange()
    {
        if (IsEditing)
        {
            this.onNameUpdated(Txt);
            IsEditing = false;
        }
        else
        {
            IsEditing = true;
        }
    }
}