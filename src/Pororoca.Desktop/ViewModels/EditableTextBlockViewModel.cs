using System;
using System.Reactive;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public sealed class EditableTextBlockViewModel : ViewModelBase
{
    private readonly Action<string> onNameUpdated;

    private string txtField;
    public string Txt
    {
        get => this.txtField;
        set => this.RaiseAndSetIfChanged(ref this.txtField, value);
    }

    private bool isEditingField;
    public bool IsEditing
    {
        get => this.isEditingField;
        set => this.RaiseAndSetIfChanged(ref this.isEditingField, value);
    }

    public ReactiveCommand<Unit, Unit> EditOrApplyTxtChangeCmd { get; }

    public EditableTextBlockViewModel(string name, Action<string> onNameUpdated)
    {
        this.onNameUpdated = onNameUpdated;
        this.txtField = name;
        this.isEditingField = false;
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