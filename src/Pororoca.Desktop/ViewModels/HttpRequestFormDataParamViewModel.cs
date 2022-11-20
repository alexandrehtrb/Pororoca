using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public sealed class HttpRequestFormDataParamViewModel : ViewModelBase
{
    public PororocaHttpRequestFormDataParamType ParamType { get; init; }

    private bool enabledField;
    public bool Enabled
    {
        get => this.enabledField;
        set => this.RaiseAndSetIfChanged(ref this.enabledField, value);
    }

    private string typeField;
    public string Type
    {
        get => this.typeField;
        set => this.RaiseAndSetIfChanged(ref this.typeField, value);
    }

    private string keyField;
    public string Key
    {
        get => this.keyField;
        set => this.RaiseAndSetIfChanged(ref this.keyField, value);
    }

    private string valueField;
    public string Value
    {
        get => this.valueField;
        set => this.RaiseAndSetIfChanged(ref this.valueField, value);
    }

    private string contentTypeField;
    public string ContentType
    {
        get => this.contentTypeField;
        set => this.RaiseAndSetIfChanged(ref this.contentTypeField, value);
    }

    public HttpRequestFormDataParamViewModel(PororocaHttpRequestFormDataParam p)
    {
        Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);

        ParamType = p.Type;
        this.enabledField = p.Enabled;
        this.typeField = ResolveParamTypeText();
        this.keyField = p.Key;
        this.valueField = p.FileSrcPath ?? p.TextValue ?? string.Empty;
        this.contentTypeField = p.ContentType;
    }

    public PororocaHttpRequestFormDataParam ToFormDataParam()
    {
        if (ParamType == PororocaHttpRequestFormDataParamType.File)
        {
            PororocaHttpRequestFormDataParam p = new(Enabled, Key);
            p.SetFileValue(Value, ContentType);
            return p;
        }
        else
        {
            PororocaHttpRequestFormDataParam p = new(Enabled, Key);
            p.SetTextValue(Value, ContentType);
            return p;
        }
    }

    private void OnLanguageChanged() =>
        Type = ResolveParamTypeText();

    private string ResolveParamTypeText() =>
        ParamType switch
        {
            PororocaHttpRequestFormDataParamType.File => Localizer.Instance["HttpRequest/BodyFormDataParamTypeFile"],
            _ => Localizer.Instance["HttpRequest/BodyFormDataParamTypeText"]
        };
}