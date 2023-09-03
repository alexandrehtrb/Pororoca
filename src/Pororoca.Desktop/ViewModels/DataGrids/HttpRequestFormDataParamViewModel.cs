using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class HttpRequestFormDataParamViewModel : ViewModelBase
{
    private readonly ObservableCollection<HttpRequestFormDataParamViewModel> parentCollection;

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

    public ReactiveCommand<Unit, Unit> RemoveParamCmd { get; }

    public HttpRequestFormDataParamViewModel(ObservableCollection<HttpRequestFormDataParamViewModel> parentCollection, PororocaHttpRequestFormDataParam p)
    {
        Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);

        this.parentCollection = parentCollection;
        ParamType = p.Type;
        Enabled = p.Enabled;
        Type = ResolveParamTypeText();
        Key = p.Key;
        Value = p.FileSrcPath ?? p.TextValue ?? string.Empty;
        ContentType = p.ContentType;
        RemoveParamCmd = ReactiveCommand.Create(RemoveParam);
    }

    public PororocaHttpRequestFormDataParam ToFormDataParam()
    {
        if (ParamType == PororocaHttpRequestFormDataParamType.File)
        {
            return PororocaHttpRequestFormDataParam.MakeFileParam(Enabled, Key, Value, ContentType);
        }
        else
        {
            return PororocaHttpRequestFormDataParam.MakeTextParam(Enabled, Key, Value, ContentType);
        }
    }

    private void OnLanguageChanged() =>
        Type = ResolveParamTypeText();

    private string ResolveParamTypeText() =>
        ParamType switch
        {
            PororocaHttpRequestFormDataParamType.File => Localizer.Instance.HttpRequest.BodyFormDataParamTypeFile,
            _ => Localizer.Instance.HttpRequest.BodyFormDataParamTypeText
        };

    private void RemoveParam() =>
        this.parentCollection.Remove(this);
}