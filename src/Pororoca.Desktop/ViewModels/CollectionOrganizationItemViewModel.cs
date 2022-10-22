using Pororoca.Desktop.Views;
using ReactiveUI;

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

    private bool canMoveUpField;
    public bool CanMoveUp
    {
        get => this.canMoveUpField;
        set => this.RaiseAndSetIfChanged(ref this.canMoveUpField, value);
    }

    private bool canMoveDownField;
    public bool CanMoveDown
    {
        get => this.canMoveDownField;
        set => this.RaiseAndSetIfChanged(ref this.canMoveDownField, value);
    }

    private EditableTextBlockViewModel nameEditableTextBlockViewDataCtxField;
    public EditableTextBlockViewModel NameEditableTextBlockViewDataCtx
    {
        get => this.nameEditableTextBlockViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.nameEditableTextBlockViewDataCtxField, value);
    }

    private string nameField;
    public string Name
    {
        get => this.nameField;
        set => this.RaiseAndSetIfChanged(ref this.nameField, value);
    }
    protected void MoveThisUp() =>
        Parent.MoveSubItem(this, MoveableItemMovementDirection.Up);

    protected void MoveThisDown() =>
        Parent.MoveSubItem(this, MoveableItemMovementDirection.Down);

    protected void Copy()
    {
        bool isMultipleCopy = CollectionsGroupDataCtx.HasMultipleItemsSelected;
        if (isMultipleCopy)
            CollectionsGroupDataCtx.CopyMultiple();
        else
            CopyThis();
    }

    protected abstract void CopyThis();

    protected void RenameThis()
    {
        NameEditableTextBlockViewDataCtx.IsEditing = true;
        Parent.OnRenameSubItemSelected(this);
    }

    private void OnNameUpdated(string newName) =>
        Name = newName;

    protected void Delete()
    {
        bool isMultipleCopy = CollectionsGroupDataCtx.HasMultipleItemsSelected;
        if (isMultipleCopy)
            CollectionsGroupDataCtx.DeleteMultiple();
        else
            DeleteThis();
    }

    public void DeleteThis() =>
        Parent.DeleteSubItem(this);
    
    public void SetAsItemInFocus(ViewModelBase vm, bool show)
    {
        if (show)
            CollectionsGroupDataCtx.CollectionGroupSelectedItem = vm;
    }

    protected CollectionOrganizationItemViewModel(ICollectionOrganizationItemParentViewModel parentVm, string name)
    {
        Parent = parentVm;
        this.nameEditableTextBlockViewDataCtxField = new(name, OnNameUpdated);
        this.nameField = name;
    }
}