using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class HttpResponseCaptureViewModel : ViewModelBase
{
    private readonly ObservableCollection<HttpResponseCaptureViewModel> parentCollection;

    public PororocaHttpResponseValueCaptureType CaptureType { get; init; }

    [Reactive]
    public string TargetVariable { get; set; }

    [Reactive]
    public string Type { get; set; }

    [Reactive]
    public string HeaderNameOrBodyPath { get; set; }

    [Reactive]
    public string? CapturedValue { get; set; }

    public ReactiveCommand<Unit, Unit> RemoveCaptureCmd { get; }

    public HttpResponseCaptureViewModel(ObservableCollection<HttpResponseCaptureViewModel> parentCollection, PororocaHttpResponseValueCapture c)
    {
        Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);

        this.parentCollection = parentCollection;
        CaptureType = c.Type;
        Type = ResolveCaptureTypeText();
        TargetVariable = c.TargetVariable;
        HeaderNameOrBodyPath =
            c.Type == PororocaHttpResponseValueCaptureType.Header ?
            (c.HeaderName ?? string.Empty) :
            (c.Path ?? string.Empty);
        RemoveCaptureCmd = ReactiveCommand.Create(RemoveCapture);
    }

    private void OnLanguageChanged() =>
        Type = ResolveCaptureTypeText();

    private string ResolveCaptureTypeText() =>
        CaptureType switch
        {
            PororocaHttpResponseValueCaptureType.Header => Localizer.Instance.HttpResponse.CaptureTypeHeader,
            _ => Localizer.Instance.HttpResponse.CaptureTypeBody
        };

    public PororocaHttpResponseValueCapture ToResponseCapture()
    {
        if (CaptureType == PororocaHttpResponseValueCaptureType.Header)
            return new(CaptureType, TargetVariable, HeaderNameOrBodyPath, null);
        else
            return new(CaptureType, TargetVariable, null, HeaderNameOrBodyPath);
    }

    private void RemoveCapture() =>
        this.parentCollection.Remove(this);
}