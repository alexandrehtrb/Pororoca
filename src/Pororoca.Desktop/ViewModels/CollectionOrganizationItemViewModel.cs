using ReactiveUI;

namespace Pororoca.Desktop.ViewModels
{
    public interface ICollectionOrganizationItemViewModel
    {
        bool CanMoveUp { get; }
        bool CanMoveDown { get; }
    }

    public abstract class CollectionOrganizationItemViewModel : ViewModelBase, ICollectionOrganizationItemViewModel
    {
        // Needs to be object variable, not static
        public ClipboardAreaViewModel ClipboardAreaDataCtx => ClipboardAreaViewModel.Singleton;
        protected ICollectionOrganizationItemParentViewModel Parent { get; }

        private bool _canMoveUp;
        public bool CanMoveUp
        {
            get => _canMoveUp;
            set
            {
                this.RaiseAndSetIfChanged(ref _canMoveUp, value);
            }
        }

        private bool _canMoveDown;
        public bool CanMoveDown
        {
            get => _canMoveDown;
            set
            {
                this.RaiseAndSetIfChanged(ref _canMoveDown, value);
            }
        }

        private EditableTextBlockViewModel _nameEditableTextBlockViewDataCtx;
        public EditableTextBlockViewModel NameEditableTextBlockViewDataCtx
        {
            get => _nameEditableTextBlockViewDataCtx;
            set
            {
                this.RaiseAndSetIfChanged(ref _nameEditableTextBlockViewDataCtx, value);
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                this.RaiseAndSetIfChanged(ref _name, value);
            }
        }
        protected void MoveThisUp() =>
            Parent.MoveSubItem(this, MoveableItemMovementDirection.Up);

        protected void MoveThisDown() =>
            Parent.MoveSubItem(this, MoveableItemMovementDirection.Down);

        protected abstract void CopyThis();

        protected void RenameThis()
        {
            NameEditableTextBlockViewDataCtx.IsEditing = true;
            Parent.OnRenameSubItemSelected(this);
        }

        private void OnNameUpdated(string newName) =>
            Name = newName;

        protected void DeleteThis() =>
            Parent.DeleteSubItem(this);

        protected CollectionOrganizationItemViewModel(ICollectionOrganizationItemParentViewModel parentVm, string name)
        {
            Parent = parentVm;
            _nameEditableTextBlockViewDataCtx = new(name, OnNameUpdated);
            _name = name;
        }
    }
}