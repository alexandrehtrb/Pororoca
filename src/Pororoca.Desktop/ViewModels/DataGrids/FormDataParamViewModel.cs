using System.Collections.ObjectModel;
using ReactiveUI.Primitives;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed partial class FormDataParamViewModel : ViewModelBase
{
    private readonly ObservableCollection<FormDataParamViewModel> parentCollection;

    public PororocaHttpRequestFormDataParamType ParamType { get; init; }

    [Reactive]
    public partial bool Enabled { get; set; }

    [Reactive]
    public partial string Type { get; set; }

    [Reactive]
    public partial string Key { get; set; }

    [Reactive]
    public partial string Value { get; set; }

    [Reactive]
    public partial string ContentType { get; set; }

    public ReactiveCommand<RxVoid, RxVoid> RemoveParamCmd { get; }

    public FormDataParamViewModel(ObservableCollection<FormDataParamViewModel> parentCollection, PororocaHttpRequestFormDataParam p)
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