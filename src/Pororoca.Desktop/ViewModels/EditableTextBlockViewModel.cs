using System;
using System.Reactive;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels
{
    public sealed class EditableTextBlockViewModel : ViewModelBase
    {
        private readonly Action<string> _onNameUpdated;

        private string _txt;
        public string Txt
        {
            get => _txt;
            set
            {
                this.RaiseAndSetIfChanged(ref _txt, value);
            }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                this.RaiseAndSetIfChanged(ref _isEditing, value);
            }
        }

        public ReactiveCommand<Unit, Unit> EditOrApplyTxtChangeCmd { get; }

        public EditableTextBlockViewModel(string name, Action<string> onNameUpdated)
        {
            _onNameUpdated = onNameUpdated;
            _txt = name;
            _isEditing = false;
            EditOrApplyTxtChangeCmd = ReactiveCommand.Create(EditOrApplyTxtChange);
        }

        public void EditOrApplyTxtChange()
        {
            if (IsEditing)
            {
                _onNameUpdated(Txt);
                IsEditing = false;
            }
            else
            {
                IsEditing = true;
            }
        }
    }
}