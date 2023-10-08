using System.Net;
using System.Reactive;
using AvaloniaEdit.Document;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.VariableCapture;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static Pororoca.Domain.Features.Common.MimeTypesDetector;

namespace Pororoca.Desktop.ViewModels;

public sealed class HttpResponseViewModel : ViewModelBase
{
    private static readonly TimeSpan oneSecond = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan oneMinute = TimeSpan.FromMinutes(1);
    private PororocaHttpResponse? res;
    private string? environmentUsedForRequest;
    private readonly CollectionViewModel colVm;
    private readonly HttpRequestViewModel parentHttpRequestVm;

    [Reactive]
    public string? ResponseStatusCodeElapsedTimeTitle { get; set; }

    // To preserve the state of the last shown response tab
    [Reactive]
    public int ResponseTabsSelectedIndex { get; set; }

    public KeyValueParamsDataGridViewModel ResponseHeadersAndTrailersTableVm { get; }

    [Reactive]
    public TextDocument? ResponseRawContentTextDocument { get; set; }

    public string? ResponseRawContent
    {
        get => ResponseRawContentTextDocument?.Text;
        set => ResponseRawContentTextDocument = new(value ?? string.Empty);
    }

    [Reactive]
    public string? ResponseRawContentType { get; set; }

    [Reactive]
    public bool IsSaveResponseBodyToFileVisible { get; set; }

    public ReactiveCommand<Unit, Unit> SaveResponseBodyToFileCmd { get; }

    [Reactive]
    public bool IsDisableTlsVerificationVisible { get; set; }

    public ReactiveCommand<Unit, Unit> DisableTlsVerificationCmd { get; }

    public HttpResponseViewModel(CollectionViewModel colVm, HttpRequestViewModel reqVm)
    {
        this.colVm = colVm;
        this.parentHttpRequestVm = reqVm;
        ResponseHeadersAndTrailersTableVm = new();
        SaveResponseBodyToFileCmd = ReactiveCommand.CreateFromTask(SaveResponseBodyToFileAsync);
        DisableTlsVerificationCmd = ReactiveCommand.Create(EnableTlsVerification);
        Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);

        UpdateWithResponse(this.res);
    }

    private void OnLanguageChanged() =>
        UpdateWithResponse(this.res);

    public async Task SaveResponseBodyToFileAsync()
    {
        string GenerateDefaultInitialFileName(string fileExtensionWithoutDot)
        {
            var receivedAtDt = this.res.ReceivedAt.DateTime;
            string reqName = this.parentHttpRequestVm.Name;
            string? envName = this.environmentUsedForRequest;
            string envLabel = envName is not null ? $"-{envName}-" : "-";
            return $"{reqName}{envLabel}response-{receivedAtDt:yyyyMMdd-HHmmss}.{fileExtensionWithoutDot}";
        }

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
                initialFileName = GenerateDefaultInitialFileName(fileExtensionWithoutDot!);
            }
            // If there is no Content-Type header in the response, let the user choose the filename and extension
            else
            {
                fileExtensionWithoutDot = "txt";
                initialFileName = GenerateDefaultInitialFileName(fileExtensionWithoutDot);
            }

            string? saveFileOutputPath = await FileExporterImporter.SelectPathForFileToBeSavedAsync(initialFileName);
            if (saveFileOutputPath != null)
            {
                await File.WriteAllBytesAsync(saveFileOutputPath, this.res.GetBodyAsBinary()!).ConfigureAwait(false);
            }
        }
    }

    private void EnableTlsVerification()
    {
        ((MainWindowViewModel)MainWindow.Instance!.DataContext!).IsSslVerificationDisabled = true;
        IsDisableTlsVerificationVisible = false;
    }

    public void UpdateWithResponse(PororocaHttpResponse? res)
    {
        if (res != null && res.Successful)
        {
            if (this.res != res)
            {
                this.res = res;
                // this is because the user might change the environment after the response was received,
                // and we need to preserve the name of the environment used for the request
                this.environmentUsedForRequest = this.colVm.GetCurrentEnvironment()?.Name;
            }
            ResponseStatusCodeElapsedTimeTitle = FormatSuccessfulResponseTitle(res.ElapsedTime, (HttpStatusCode)res.StatusCode!);
            UpdateHeadersAndTrailers(res.Headers, res.Trailers);
            // response content type needs to be always set first, because when the content is updated,
            // it triggers the syntax update, that checks the content type
            // if content type is set after, the syntax will not change after content is updated
            ResponseRawContentType = res.ContentType;
            ResponseRawContent = res.CanDisplayTextBody ?
                                 res.GetBodyAsPrettyText(Localizer.Instance.HttpResponse.BodyCouldNotReadAsUTF8) :
                                 string.Format(Localizer.Instance.HttpResponse.BodyContentBinaryNotShown, res.GetBodyAsBinary()!.Length);
            IsSaveResponseBodyToFileVisible = res.HasBody;
            IsDisableTlsVerificationVisible = false;
            CaptureResponseValues(res);
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

            bool isSslVerificationDisabled = ((MainWindowViewModel)MainWindow.Instance!.DataContext!).IsSslVerificationDisabled;
            IsDisableTlsVerificationVisible = !isSslVerificationDisabled && res.FailedDueToTlsVerification;
        }
        else if (this.res == null) // No response obtained yet.
        {
            ResponseStatusCodeElapsedTimeTitle = Localizer.Instance.HttpResponse.SectionTitle;
            IsDisableTlsVerificationVisible = false;
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

    private void CaptureResponseValues(PororocaHttpResponse res)
    {
        var egvm = (EnvironmentsGroupViewModel)this.colVm.Items.First(i => i is EnvironmentsGroupViewModel);
        var env = egvm.Items.FirstOrDefault(e => e.Name == this.environmentUsedForRequest);
        if (env is not null)
        {
            var captures = this.parentHttpRequestVm.ResCapturesTableVm.Items;
            foreach (var capture in captures)
            {
                var envVar = env.VariablesTableVm.Items.FirstOrDefault(v => v.Key == capture.TargetVariable);
                string? capturedValue = res?.CaptureValue(capture.ToResponseCapture());
                if (capturedValue is not null)
                {
                    capture.CapturedValue = capturedValue;
                    if (envVar is null)
                    {
                        envVar = new(env.VariablesTableVm.Items, new(true, capture.TargetVariable, capturedValue, true));
                        env.VariablesTableVm.Items.Add(envVar);
                    }
                    else
                    {
                        envVar.Value = capturedValue;
                    }
                }
            }
        }
    }

    private static string FormatSuccessfulResponseTitle(TimeSpan elapsedTime, HttpStatusCode statusCode) =>
        string.Format(Localizer.Instance.HttpResponse.SectionTitleSuccessfulFormat,
                      FormatHttpStatusCode(statusCode),
                      FormatElapsedTime(elapsedTime));

    private static string FormatFailedResponseTitle(TimeSpan elapsedTime) =>
        string.Format(Localizer.Instance.HttpResponse.SectionTitleFailedFormat,
                      FormatElapsedTime(elapsedTime));

    private static string FormatHttpStatusCode(HttpStatusCode statusCode) =>
        $"{(int)statusCode} {Enum.GetName(statusCode)}";

    private static string FormatElapsedTime(TimeSpan elapsedTime) =>
        elapsedTime < oneSecond ?
        string.Format(Localizer.Instance.HttpResponse.ElapsedTimeMillisecondsFormat, (int)elapsedTime.TotalMilliseconds) :
        elapsedTime < oneMinute ? // More or equal than one second, but less than one minute
        string.Format(Localizer.Instance.HttpResponse.ElapsedTimeSecondsFormat, elapsedTime.TotalSeconds) : // TODO: Format digit separator according to language
        string.Format(Localizer.Instance.HttpResponse.ElapsedTimeMinutesFormat, elapsedTime.Minutes, elapsedTime.Seconds);

}