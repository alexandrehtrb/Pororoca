using ReactiveUI;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels
{
    public class KeyValueParamViewModel : ViewModelBase
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

        public KeyValueParamViewModel(PororocaKeyValueParam p) : this(p.Enabled, p.Key, p.Value ?? string.Empty)
        {
        }

        public KeyValueParamViewModel(bool enabled, string key, string value)
        {
            _enabled = enabled;
            _key = key;
            _value = value;
        }

        public PororocaKeyValueParam ToKeyValueParam() =>
            new(_enabled, _key, _value);
    }
}