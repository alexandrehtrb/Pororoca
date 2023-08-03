using System.Collections.ObjectModel;
using System.Net;
using System.Reactive;
using AvaloniaEdit.Document;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static Pororoca.Domain.Features.Common.MimeTypesDetector;

namespace Pororoca.Desktop.ViewModels;

public sealed class HttpResponseViewModel : ViewModelBase
{
    private static readonly TimeSpan oneSecond = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan oneMinute = TimeSpan.FromMinutes(1);
    private PororocaHttpResponse? res;

    [Reactive]
    public string? ResponseStatusCodeElapsedTimeTitle { get; set; }

    // To preserve the state of the last shown response tab
    [Reactive]
    public int ResponseTabsSelectedIndex { get; set; }

    public ObservableCollection<KeyValueParamViewModel> ResponseHeadersAndTrailers { get; }

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

    public HttpResponseViewModel()
    {
        ResponseHeadersAndTrailers = new();
        SaveResponseBodyToFileCmd = ReactiveCommand.CreateFromTask(SaveResponseBodyToFileAsync);
        DisableTlsVerificationCmd = ReactiveCommand.Create(DisableTlsVerification);
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
            return $"response-{receivedAtDt:yyyyMMdd-HHmmss}.{fileExtensionWithoutDot}";
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

    private void DisableTlsVerification()
    {
        ((MainWindowViewModel)MainWindow.Instance!.DataContext!).IsSslVerificationDisabled = true;
        IsDisableTlsVerificationVisible = false;
    }

    public void UpdateWithResponse(PororocaHttpResponse? res)
    {
        if (res != null && res.Successful)
        {
            // Gambiarra to temporarily fix this bug:
            // https://github.com/AvaloniaUI/Avalonia/issues/9527
            // When a response is received but the selected tab is Body, 
            // the Response Headers DataGrid would not be updated.
            // To remediate this, we will first switch to Headers tab, then go back to Body tab.
            bool wasShowingResponseBody = (ResponseTabsSelectedIndex == 1);
            if (wasShowingResponseBody)
            {
                ResponseTabsSelectedIndex = 0; // switch to Headers tab
            }

            this.res = res;
            ResponseStatusCodeElapsedTimeTitle = FormatSuccessfulResponseTitle(res.ElapsedTime, (HttpStatusCode)res.StatusCode!);
            UpdateHeadersAndTrailers(res.Headers, res.Trailers);
            // response content type needs to be always set first, because when the content is updated,
            // it triggers the syntax update, that checks the content type
            // if content type is set after, the syntax will not change after content is updated
            ResponseRawContentType = res.ContentType;
            ResponseRawContent = res.CanDisplayTextBody ?
                                 res.GetBodyAsText(Localizer.Instance.HttpResponse.BodyCouldNotReadAsUTF8) :
                                 string.Format(Localizer.Instance.HttpResponse.BodyContentBinaryNotShown, res.GetBodyAsBinary()!.Length);
            IsSaveResponseBodyToFileVisible = res.HasBody;
            IsDisableTlsVerificationVisible = false;

            if (wasShowingResponseBody)
            {
                ResponseTabsSelectedIndex = 1; // switch back to Body tab
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
        ResponseHeadersAndTrailers.Clear();
        if (resHeaders != null)
        {
            foreach (var kvp in resHeaders)
            {
                ResponseHeadersAndTrailers.Add(new(ResponseHeadersAndTrailers, true, kvp.Key, kvp.Value));
            }
        }
        if (resTrailers != null)
        {
            foreach (var kvp in resTrailers)
            {
                ResponseHeadersAndTrailers.Add(new(ResponseHeadersAndTrailers, true, kvp.Key, kvp.Value));
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