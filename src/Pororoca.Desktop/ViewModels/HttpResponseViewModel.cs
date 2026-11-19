using System.Diagnostics;
using System.Net;
using System.Text;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.SourceGenerators;
using static Pororoca.Desktop.Localization.TimeTextFormatter;
using static Pororoca.Domain.Features.Common.HttpStatusCodeFormatter;
using static Pororoca.Domain.Features.Common.MimeTypesDetector;
using static Pororoca.Domain.Features.ExportLog.HttpLogExporter;

namespace Pororoca.Desktop.ViewModels;

public sealed partial class HttpResponseViewModel : ViewModelBase
{
    private PororocaHttpResponse? res;
    private CancellationToken? cancellationToken;
    private Action? setRequestFinishedAction;
    private string? environmentUsedForRequest;
    private readonly CollectionViewModel colVm;
    private string? requestName;
    private IEnumerable<HttpResponseCaptureViewModel>? responseCaptures;

    [Reactive]
    public partial string? ResponseStatusCodeElapsedTimeTitle { get; set; }

    // To preserve the state of the last shown response tab
    [Reactive]
    public partial int ResponseTabsSelectedIndex { get; set; }

    public KeyValueParamsDataGridViewModel ResponseHeadersAndTrailersTableVm { get; }

    [Reactive]
    public partial TextDocument? ResponseRawContentTextDocument { get; set; }

    public string? ResponseRawContent
    {
        get => ResponseRawContentTextDocument?.Text;
        set => ResponseRawContentTextDocument = new(value ?? string.Empty);
    }

    [Reactive]
    public partial string? ResponseRawContentType { get; set; }

    [Reactive]
    public partial bool IsSaveResponseBodyToFileVisible { get; set; }

    [Reactive]
    public partial bool IsExportLogFileVisible { get; set; }

    public ReactiveCommand<RxVoid, RxVoid> SaveResponseBodyToFileCmd { get; }

    public ReactiveCommand<RxVoid, RxVoid> ExportLogFileCmd { get; }

    [Reactive]
    public partial bool IsDisableTlsVerificationVisible { get; set; }

    public ReactiveCommand<RxVoid, RxVoid> DisableTlsVerificationCmd { get; }

    public ReactiveCommand<RxVoid, RxVoid> ExecuteCapturesCmd { get; }

    public HttpResponseViewModel(CollectionViewModel colVm)
    {
        this.colVm = colVm;
        ResponseHeadersAndTrailersTableVm = new();
        SaveResponseBodyToFileCmd = ReactiveCommand.CreateFromTask(SaveResponseBodyToFileAsync);
        ExportLogFileCmd = ReactiveCommand.CreateFromTask(ExportLogToFileAsync);
        DisableTlsVerificationCmd = ReactiveCommand.Create(DisableTlsVerification);
        ExecuteCapturesCmd = ReactiveCommand.Create(ExecuteResponseCaptures);
        Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);

        UpdateWithResponse(string.Empty, this.res, null, null, CancellationToken.None);
    }

    private void OnLanguageChanged() =>
        UpdateWithResponse(this.requestName ?? string.Empty, this.res, this.responseCaptures, this.setRequestFinishedAction, this.cancellationToken);

    private string GenerateDefaultResponseInitialFileName(string fileExtensionWithoutDot)
    {
        var receivedAtDt = this.res!.ReceivedAt.DateTime;
        string? envName = this.environmentUsedForRequest;
        string envLabel = envName is not null ? $"-{envName}" : string.Empty;
        return $"response-{this.requestName!}{envLabel}-{receivedAtDt:yyyyMMdd-HHmmss}.{fileExtensionWithoutDot}";
    }

    private string GenerateDefaultLogInitialFileName()
    {
        var receivedAtDt = this.res!.ReceivedAt.DateTime;
        string? envName = this.environmentUsedForRequest;
        string envLabel = envName is not null ? $"-{envName}" : string.Empty;
        return $"log-{this.requestName!}{envLabel}-{receivedAtDt:yyyyMMdd-HHmmss}.log";
    }

    public string? GetFileExtensionForResponseBody()
    {
        if (this.res != null && this.res.HasBody && this.res.ContentType is not null && TryFindFileExtensionForContentType(this.res.ContentType, out string? fileExtensionWithoutDot))
        {
            return fileExtensionWithoutDot;
        }
        else
        {
            return null;
        }
    }

    public async Task SaveResponseBodyToFileAsync()
    {
        if (this.res != null && this.res.HasBody)
        {
            string? contentDispositionFileName = this.res.GetContentDispositionFileName();
            string? contentType = this.res.ContentType;
            string? initialFileName;
            // If response has Content-Disposition header with filename property, then use it for the file extension
            if (!string.IsNullOrWhiteSpace(contentDispositionFileName))
            {
                initialFileName = contentDispositionFileName;
            }
            // Otherwise, use response's Content-Type header for file extension
            else if (contentType != null && TryFindFileExtensionForContentType(contentType, out string? fileExtensionWithoutDot))
            {
                initialFileName = GenerateDefaultResponseInitialFileName(fileExtensionWithoutDot!);
            }
            // If there is no Content-Type header in the response, let the user choose the filename and extension
            else
            {
                fileExtensionWithoutDot = "txt";
                initialFileName = GenerateDefaultResponseInitialFileName(fileExtensionWithoutDot);
            }

            string? saveFileOutputPath = await FileExporterImporter.SelectPathForFileToBeSavedAsync(initialFileName);
            if (saveFileOutputPath != null)
            {
                await File.WriteAllBytesAsync(saveFileOutputPath, this.res.GetBodyAsBinary()!).ConfigureAwait(false);
            }
        }
    }

    public async Task ExportLogToFileAsync()
    {
        if (this.res != null)
        {
            string initialFileName = GenerateDefaultLogInitialFileName();
            string? saveFileOutputPath = await FileExporterImporter.SelectPathForFileToBeSavedAsync(initialFileName);
            if (saveFileOutputPath != null)
            {
                string logTxt = ProduceHttpLog(this.res);
                await File.WriteAllTextAsync(saveFileOutputPath, logTxt).ConfigureAwait(false);
            }
        }
    }

    private void DisableTlsVerification()
    {
        MainWindowVm.IsSslVerificationDisabled = true;
        IsDisableTlsVerificationVisible = false;
    }

    public void UpdateWithResponse(string requestName, PororocaHttpResponse? res, IEnumerable<HttpResponseCaptureViewModel>? responseCaptures, Action? setRequestFinishedAction, CancellationToken? cancellationToken)
    {
        this.requestName = requestName;
        this.responseCaptures = responseCaptures;
        this.cancellationToken = cancellationToken;
        this.setRequestFinishedAction = setRequestFinishedAction;

        if (res != null && res.Successful)
        {
            if (this.res != res)
            {
                this.res = res;
                // this is because the user might change the environment after the response was received,
                // and we need to preserve the name of the environment used for the request
                this.environmentUsedForRequest = this.colVm.CurrentEnvironmentVm?.Name;

                // this below is to avoid spawning more than one server-sent event listener action
                // if the user changes the app language whilst receiving server-sent events.
                if (res.IsStreamed)
                {
                    Dispatcher.UIThread.Post(async () => await UpdateResponseBodyWithServerSentEventsAsync(res, (CancellationToken)cancellationToken!));
                }
            }

            UpdateHeadersAndTrailers(res.Headers, res.Trailers);
            // response content type needs to be always set first, because when the content is updated,
            // it triggers the syntax update, that checks the content type
            // if content type is set after, the syntax will not change after content is updated
            ResponseRawContentType = res.ContentType;

            if (res.IsStreamed)
            {
                ResponseStatusCodeElapsedTimeTitle = FormatReceivingStreamResponseTitle((HttpStatusCode)res.StatusCode!);                
                IsSaveResponseBodyToFileVisible = false;
                IsExportLogFileVisible = false;
                IsDisableTlsVerificationVisible = false;
            }
            else
            {
                ResponseStatusCodeElapsedTimeTitle = FormatSuccessfulResponseTitle(res.ElapsedTime, (HttpStatusCode)res.StatusCode!);
                ResponseRawContent = res.CanDisplayTextBody ?
                                     res.GetBodyAsPrettyText(Localizer.Instance.HttpResponse.BodyCouldNotReadAsUTF8) :
                                     string.Format(Localizer.Instance.HttpResponse.BodyContentBinaryNotShown, res.GetBodyAsBinary()!.Length);
                IsSaveResponseBodyToFileVisible = res.HasBody;
                IsExportLogFileVisible = true;
                IsDisableTlsVerificationVisible = false;
                ExecuteResponseCaptures();
            }
        }
        else if (res != null && !res.WasCancelled) // Not success, but also not cancelled. If cancelled, do nothing.
        {
            this.res = res;
            // TODO: Improve this, do not use fixed values to resolve index
            ResponseTabsSelectedIndex = 1; // Show exception message
            ResponseStatusCodeElapsedTimeTitle = FormatFailedResponseTitle(res.ElapsedTime);
            UpdateHeadersAndTrailers(res.Headers, res.Trailers);
            ResponseRawContentType = null;
            ResponseRawContent = res.Exception!.ToString();
            IsSaveResponseBodyToFileVisible = false;
            IsExportLogFileVisible = false;
            bool isSslVerificationDisabled = MainWindowVm.IsSslVerificationDisabled;
            IsDisableTlsVerificationVisible = !isSslVerificationDisabled && res.FailedDueToTlsVerification;
        }
        else if (this.res == null || res == null) // No response obtained yet, or clearing up.
        {
            ResponseStatusCodeElapsedTimeTitle = Localizer.Instance.HttpResponse.SectionTitle;
            IsDisableTlsVerificationVisible = false;
            UpdateHeadersAndTrailers(null, null);
            ResponseRawContentType = null;
            ResponseRawContent = string.Empty;
        }
    }

    private void UpdateHeadersAndTrailers(IEnumerable<KeyValuePair<string, string>>? resHeaders, IEnumerable<KeyValuePair<string, string>>? resTrailers)
    {
        var tableItems = ResponseHeadersAndTrailersTableVm.Items;
        tableItems.Clear();
        if (resHeaders != null)
        {
            foreach (var kvp in resHeaders)
            {
                tableItems.Add(new(tableItems, true, kvp.Key, kvp.Value));
            }
        }
        if (resTrailers != null)
        {
            foreach (var kvp in resTrailers)
            {
                tableItems.Add(new(tableItems, true, kvp.Key, kvp.Value));
            }
        }
    }

    private async Task UpdateResponseBodyWithServerSentEventsAsync(PororocaHttpResponse res, CancellationToken cancellationToken)
    {
        // initial setup of TextDocument
        ResponseRawContentTextDocument = new();
        this.cancellationToken = cancellationToken;
        try
        {
            // Scenario 1: The Stream is already completed.
            // This may happen in repeaters. The user clicks on an HttpRepetitionResult
            // to see the response body, but it completed even before this response was
            // the one selected and being watched.
            // In this case, it's not necessary to drain the StreamedBodyReader;
            // just use the BinaryBody that already contains the full response body.
            if (res.HasBody)
            {
                // if the stream is already completed, then just set the complete binary body
                // into the response body text document
                ResponseRawContentTextDocument.Insert(0, res.GetBodyAsString());
            }
            else
            {
                // Scenario 2: The Stream is ongoing.
                // This is the default for most cases. The user is on HttpRequestView screen,
                // clicks on Send, and the Stream starts to arrive.
                // Each received part must be appended to the current text document.
                // Let's drain the stream as the parts arrive.
                await foreach (string part in res.StreamedBodyReader!.ReadAllAsync(cancellationToken))
                {
                    ResponseRawContentTextDocument.Insert(ResponseRawContentTextDocument.TextLength, part);
                }
            }
        }
        catch
        {
        }
        finally
        {
            ResponseStatusCodeElapsedTimeTitle = FormatSuccessfulResponseTitle(res.ElapsedTime, (HttpStatusCode)res.StatusCode!);
            IsSaveResponseBodyToFileVisible = true;
            IsExportLogFileVisible = true;
            this.setRequestFinishedAction?.Invoke();
        }
    }

    private void ExecuteResponseCaptures()
    {
        if (this.res == null || !this.res.Successful || this.responseCaptures is null)
            return;

        var colVarsVm = this.colVm.CollectionVariablesVm;
        var envVm = this.colVm.CurrentEnvironmentVm;
        // if an environment is selected, then save captures into its variables
        // otherwise, save captures into collection variables
        var targetVars = (envVm?.VariablesTableVm ?? colVarsVm.VariablesTableVm).Items;

        foreach (var capture in this.responseCaptures)
        {
            bool shouldNotSaveAsVariable =
                string.IsNullOrWhiteSpace(capture.TargetVariable) ||
                (capture.TargetVariable.Length > 0 && capture.TargetVariable[0] == '_');

            var targetVar = targetVars.FirstOrDefault(v => v.Key == capture.TargetVariable);
            string? capturedValue = this.res?.CaptureValue(capture.ToResponseCapture());
            if (capturedValue is not null)
            {
                capture.CapturedValue = capturedValue;
                if (shouldNotSaveAsVariable)
                {
                    continue;
                }
                else if (targetVar is null)
                {
                    targetVar = new(targetVars, new(true, capture.TargetVariable, capturedValue, true));
                    targetVars.Add(targetVar);
                }
                else
                {
                    targetVar.Value = capturedValue;
                }
            }
        }
    }

    private static string FormatSuccessfulResponseTitle(TimeSpan elapsedTime, HttpStatusCode statusCode) =>
        string.Format(Localizer.Instance.HttpResponse.SectionTitleSuccessfulFormat,
                      FormatHttpStatusCodeText(statusCode),
                      FormatTimeText(elapsedTime));

    private static string FormatReceivingStreamResponseTitle(HttpStatusCode statusCode) =>
        string.Format(Localizer.Instance.HttpResponse.SectionTitleReceivingStreamFormat,
                      FormatHttpStatusCodeText(statusCode));

    private static string FormatFailedResponseTitle(TimeSpan elapsedTime) =>
        string.Format(Localizer.Instance.HttpResponse.SectionTitleFailedFormat,
                      FormatTimeText(elapsedTime));
}