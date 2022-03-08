using ReactiveUI;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Desktop.Localization;

namespace Pororoca.Desktop.ViewModels
{
    public sealed class RequestFormDataParamViewModel : ViewModelBase
    {
        public PororocaRequestFormDataParamType ParamType { get; init; }

        private bool _enabled;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _enabled, value);
            }
        }

        private string _type;
        public string Type
        {
            get => _type;
            set
            {
                this.RaiseAndSetIfChanged(ref _type, value);
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

        private string _contentType;
        public string ContentType
        {
            get => _contentType;
            set
            {
                this.RaiseAndSetIfChanged(ref _contentType, value);
            }
        }

        public RequestFormDataParamViewModel(PororocaRequestFormDataParam p)
        {
            Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);

            ParamType = p.Type;
            _enabled = p.Enabled;
            _type = ResolveParamTypeText();
            _key = p.Key;
            _value = p.FileSrcPath ?? p.TextValue ?? string.Empty;
            _contentType = p.ContentType;
        }

        public PororocaRequestFormDataParam ToFormDataParam()
        {
            if (ParamType == PororocaRequestFormDataParamType.File)
            {
                PororocaRequestFormDataParam p = new(Enabled, Key);
                p.SetFileValue(Value, _contentType);
                return p;
            }
            else
            {
                PororocaRequestFormDataParam p = new(Enabled, Key);
                p.SetTextValue(Value, _contentType);
                return p;
            }
        }

        private void OnLanguageChanged() =>
            Type = ResolveParamTypeText();

        private string ResolveParamTypeText() =>
            ParamType switch
            {
                PororocaRequestFormDataParamType.File => Localizer.Instance["Request/BodyFormDataParamTypeFile"],
                _ => Localizer.Instance["Request/BodyFormDataParamTypeText"]
            };
    }
}