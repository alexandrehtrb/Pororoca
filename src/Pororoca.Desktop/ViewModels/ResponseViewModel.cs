using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using static Pororoca.Domain.Features.Common.MimeTypesDetector;

namespace Pororoca.Desktop.ViewModels;

public sealed class ResponseViewModel : ViewModelBase
{
    private static readonly TimeSpan oneSecond = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan oneMinute = TimeSpan.FromMinutes(1);
    private PororocaResponse? res;

    private string? responseStatusCodeElapsedTimeTitleField;
    public string? ResponseStatusCodeElapsedTimeTitle
    {
        get => this.responseStatusCodeElapsedTimeTitleField;
        set => this.RaiseAndSetIfChanged(ref this.responseStatusCodeElapsedTimeTitleField, value);
    }

    private int responseTabsSelectedIndexField;
    public int ResponseTabsSelectedIndex // To preserve the state of the last shown response tab
    {
        get => this.responseTabsSelectedIndexField;
        set => this.RaiseAndSetIfChanged(ref this.responseTabsSelectedIndexField, value);
    }

    public ObservableCollection<KeyValueParamViewModel> ResponseHeaders { get; }

    private string? responseRawContentField;
    public string? ResponseRawContent
    {
        get => this.responseRawContentField;
        set => this.RaiseAndSetIfChanged(ref this.responseRawContentField, value);
    }

    private bool isSaveResponseBodyToFileVisibleField;
    public bool IsSaveResponseBodyToFileVisible
    {
        get => this.isSaveResponseBodyToFileVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isSaveResponseBodyToFileVisibleField, value);
    }

    public ReactiveCommand<Unit, Unit> SaveResponseBodyToFileCmd { get; }

    public ResponseViewModel()
    {
        ResponseHeaders = new();
        SaveResponseBodyToFileCmd = ReactiveCommand.CreateFromTask(SaveResponseBodyToFileAsync);
        Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);

        UpdateWithResponse(this.res);
    }

    private void OnLanguageChanged() =>
        UpdateWithResponse(this.res);

    private async Task SaveResponseBodyToFileAsync()
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

            SaveFileDialog saveFileDialog = new()
            {
                InitialFileName = initialFileName
            };

            string? saveFileOutputPath = await saveFileDialog.ShowAsync(MainWindow.Instance!);
            if (saveFileOutputPath != null)
            {
                await File.WriteAllBytesAsync(saveFileOutputPath, this.res.GetBodyAsBinary()!).ConfigureAwait(false);
            }
        }
    }

    public void UpdateWithResponse(PororocaResponse? res)
    {
        if (res != null && res.Successful)
        {
            this.res = res;
            ResponseStatusCodeElapsedTimeTitle = FormatSuccessfulResponseTitle(res.ElapsedTime, (HttpStatusCode)res.StatusCode!);
            UpdateHeaders(res.Headers);
            ResponseRawContent = res.CanDisplayTextBody ? res.GetBodyAsText() : string.Format(Localizer.Instance["Response/BodyContentBinaryNotShown"], res.GetBodyAsBinary()!.Length);
            IsSaveResponseBodyToFileVisible = res.HasBody;
        }
        else if (res != null && !res.WasCancelled) // Not success, but also not cancelled. If cancelled, do nothing.
        {
            this.res = res;
            ResponseStatusCodeElapsedTimeTitle = FormatFailedResponseTitle(res.ElapsedTime);
            UpdateHeaders(res.Headers);
            ResponseRawContent = res.Exception!.ToString();
            IsSaveResponseBodyToFileVisible = false;
        }
        else if (this.res == null) // No response obtained yet.
        {
            ResponseStatusCodeElapsedTimeTitle = Localizer.Instance["Response/SectionTitle"];

            ResponseRawContent = string.Empty;
        }
    }

    private void UpdateHeaders(IEnumerable<KeyValuePair<string, string>>? resHeaders)
    {
        ResponseHeaders.Clear();
        if (resHeaders != null)
        {
            foreach (var kvp in resHeaders)
            {
                PororocaKeyValueParam pkvp = new(true, kvp.Key, kvp.Value);
                ResponseHeaders.Add(new KeyValueParamViewModel(pkvp));
            }
        }
    }

    private static string FormatSuccessfulResponseTitle(TimeSpan elapsedTime, HttpStatusCode statusCode) =>
        string.Format(Localizer.Instance["Response/SectionTitleSuccessfulFormat"],
                      FormatHttpStatusCode(statusCode),
                      FormatElapsedTime(elapsedTime));

    private static string FormatFailedResponseTitle(TimeSpan elapsedTime) =>
        string.Format(Localizer.Instance["Response/SectionTitleFailedFormat"],
                      FormatElapsedTime(elapsedTime));

    private static string FormatHttpStatusCode(HttpStatusCode statusCode) =>
        $"{(int)statusCode} {Enum.GetName(statusCode)}";

    private static string FormatElapsedTime(TimeSpan elapsedTime) =>
        elapsedTime < oneSecond ?
        string.Format(Localizer.Instance["Response/ElapsedTimeMillisecondsFormat"], (int)elapsedTime.TotalMilliseconds) :
        elapsedTime < oneMinute ? // More or equal than one second, but less than one minute
        string.Format(Localizer.Instance["Response/ElapsedTimeSecondsFormat"], elapsedTime.TotalSeconds) : // TODO: Format digit separator according to language
        string.Format(Localizer.Instance["Response/ElapsedTimeMinutesFormat"], elapsedTime.Minutes, elapsedTime.Seconds);

}