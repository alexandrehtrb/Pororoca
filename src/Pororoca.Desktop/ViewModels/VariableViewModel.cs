using ReactiveUI;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels
{
    public class VariableViewModel : ViewModelBase
    {
        private bool _enabled;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _enabled, value);
            }
        }

        private string _key;
        public string Key
        {
            get => _key;
            set
            {
                this.RaiseAndSetIfChanged(ref _key, value);
            }
        }

        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                this.RaiseAndSetIfChanged(ref _value, value);
            }
        }

        private bool _isSecret;
        public bool IsSecret
        {
            get => _isSecret;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSecret, value);
            }
        }

        public VariableViewModel(PororocaVariable v) : this(v.Enabled, v.Key, v.Value ?? string.Empty, v.IsSecret)
        {
        }

        public VariableViewModel(bool enabled, string key, string value, bool isSecret)
        {
            _enabled = enabled;
            _key = key;
            _value = value;
            _isSecret = isSecret;
        }

        public PororocaVariable ToVariable() =>
            new(_enabled, _key, _value, _isSecret);
    }
}