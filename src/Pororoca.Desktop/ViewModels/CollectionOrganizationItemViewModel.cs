using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public abstract class CollectionOrganizationItemViewModel : ViewModelBase
{
    // Needs to be object variable, not static
    // TODO: Should it receive this via injection?
    public CollectionsGroupViewModel CollectionsGroupDataCtx =>
        MainWindowVm.CollectionsGroupViewDataCtx;

    public ICollectionOrganizationItemParentViewModel Parent { get; set; }

    [Reactive]
    public bool CanMoveUp { get; set; }

    [Reactive]
    public bool CanMoveDown { get; set; }

    [Reactive]
    public EditableTextBlockViewModel NameEditableVm { get; set; }

    [Reactive]
    public string Name { get; set; }

    public void MoveThisUp() =>
        Parent.MoveSubItemUp(this);

    public void MoveThisDown() =>
        Parent.MoveSubItemDown(this);

    public void RenameThis()
    {
        if (NameEditableVm.IsEditing == false)
        {
            NameEditableVm.IsEditing = true;
        }
        else
        {
            NameEditableVm.EditOrApplyTxtChange();
        }
    }

    protected virtual void OnNameUpdated(string newName) =>
        Name = newName;

    public virtual void DeleteThis() =>
        Parent.DeleteSubItem(this);

    public void SetAsItemInFocus(CollectionOrganizationItemViewModel vm, bool show)
    {
        if (show)
            CollectionsGroupDataCtx.CollectionGroupSelectedItem = vm;
    }

    public bool IsDescendantOf(CollectionOrganizationItemViewModel possibleAncestor)
    {
        var current = Parent;
        while (current is not null)
        {
            if (current == possibleAncestor)
                return true;
            else if (current is CollectionOrganizationItemViewModel itemVm)
                current = itemVm.Parent; // above is to avoid casting into MainWindowViewModel
            else
                break;
        }
        return false;
    }

    protected CollectionOrganizationItemViewModel(ICollectionOrganizationItemParentViewModel parentVm, string name)
    {
        Parent = parentVm;
        NameEditableVm = new(name, OnNameUpdated);
        Name = name;
    }
}