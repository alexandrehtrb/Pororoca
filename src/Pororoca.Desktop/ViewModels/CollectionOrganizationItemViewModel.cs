using Pororoca.Desktop.Views;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public interface ICollectionOrganizationItemViewModel
{
    bool CanMoveUp { get; }
    bool CanMoveDown { get; }
}

public abstract class CollectionOrganizationItemViewModel : ViewModelBase, ICollectionOrganizationItemViewModel
{
    // Needs to be object variable, not static
    // TODO: Should it receive this via injection?
    public CollectionsGroupViewModel CollectionsGroupDataCtx =>
        ((MainWindowViewModel)MainWindow.Instance!.DataContext!).CollectionsGroupViewDataCtx;

    public ICollectionOrganizationItemParentViewModel Parent { get; }

    [Reactive]
    public bool CanMoveUp { get; set; }

    [Reactive]
    public bool CanMoveDown { get; set; }

    [Reactive]
    public EditableTextBlockViewModel NameEditableTextBlockViewDataCtx { get; set; }

    [Reactive]
    public string Name { get; set; }

    public void MoveThisUp() =>
        Parent.MoveSubItem(this, MoveableItemMovementDirection.Up);

    public void MoveThisDown() =>
        Parent.MoveSubItem(this, MoveableItemMovementDirection.Down);

    protected abstract void CopyThis();

    public void RenameThis()
    {
        if (NameEditableTextBlockViewDataCtx.IsEditing == false)
        {
            NameEditableTextBlockViewDataCtx.IsEditing = true;
            Parent.OnRenameSubItemSelected(this);
        }
        else
        {
            NameEditableTextBlockViewDataCtx.EditOrApplyTxtChange();
        }
    }

    protected virtual void OnNameUpdated(string newName) =>
        Name = newName;

    public virtual void DeleteThis() =>
        Parent.DeleteSubItem(this);

    public void SetAsItemInFocus(ViewModelBase vm, bool show)
    {
        if (show)
            CollectionsGroupDataCtx.CollectionGroupSelectedItem = vm;
    }

    protected CollectionOrganizationItemViewModel(ICollectionOrganizationItemParentViewModel parentVm, string name)
    {
        Parent = parentVm;
        NameEditableTextBlockViewDataCtx = new(name, OnNameUpdated);
        Name = name;
    }
}