using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class HttpRequestFormDataParamViewModel : ViewModelBase
{
    public PororocaHttpRequestFormDataParamType ParamType { get; init; }

    [Reactive]
    public bool Enabled { get; set; }

    [Reactive]
    public string Type { get; set; }

    [Reactive]
    public string Key { get; set; }

    [Reactive]
    public string Value { get; set; }

    [Reactive]
    public string ContentType { get; set; }

    public HttpRequestFormDataParamViewModel(PororocaHttpRequestFormDataParam p)
    {
        Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);

        ParamType = p.Type;
        Enabled = p.Enabled;
        Type = ResolveParamTypeText();
        Key = p.Key;
        Value = p.FileSrcPath ?? p.TextValue ?? string.Empty;
        ContentType = p.ContentType;
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