using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.MimeTypesDetector;

namespace Pororoca.Desktop.ViewModels
{
    public sealed class ResponseViewModel : ViewModelBase
    {
        private static readonly TimeSpan _oneSecond = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan _oneMinute = TimeSpan.FromMinutes(1);
        private PororocaResponse? _res;

        private string? _responseStatusCodeElapsedTimeTitle;
        public string? ResponseStatusCodeElapsedTimeTitle
        {
            get => _responseStatusCodeElapsedTimeTitle;
            set
            {
                this.RaiseAndSetIfChanged(ref _responseStatusCodeElapsedTimeTitle, value);
            }
        }

        private int _responseTabsSelectedIndex;
        public int ResponseTabsSelectedIndex // To preserve the state of the last shown response tab
        {
            get => _responseTabsSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _responseTabsSelectedIndex, value);
            }
        }

        public ObservableCollection<KeyValueParamViewModel> ResponseHeaders { get; }

        private string? _responseRawContent;
        public string? ResponseRawContent
        {
            get => _responseRawContent;
            set
            {
                this.RaiseAndSetIfChanged(ref _responseRawContent, value);
            }
        }

        private bool _isSaveResponseBodyToFileVisible;
        public bool IsSaveResponseBodyToFileVisible
        {
            get => _isSaveResponseBodyToFileVisible;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSaveResponseBodyToFileVisible, value);
            }
        }

        public ReactiveCommand<Unit, Unit> SaveResponseBodyToFileCmd { get; }

        public ResponseViewModel()
        {
            ResponseHeaders = new();
            SaveResponseBodyToFileCmd = ReactiveCommand.CreateFromTask(SaveResponseBodyToFileAsync);
            Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);

            UpdateWithResponse(_res);
        }

        private void OnLanguageChanged() =>
            UpdateWithResponse(_res);

        private async Task SaveResponseBodyToFileAsync()
        {
            string GenerateDefaultInitialFileName(string fileExtensionWithoutDot)
            {
                DateTime receivedAtDt = _res.ReceivedAt.DateTime;
                return $"response-{receivedAtDt:yyyyMMdd-HHmmss}.{fileExtensionWithoutDot}";
            }

            if (_res != null && _res.HasBody)
            {
                string? contentDispositionFileName = _res.GetContentDispositionFileName();
                string? contentType = _res.ContentType;
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
                    await File.WriteAllBytesAsync(saveFileOutputPath, _res.GetBodyAsBinary()!).ConfigureAwait(false);
                }
            }
        }

        public void UpdateWithResponse(PororocaResponse? res)
        {
            if (res != null && res.Successful)
            {
                _res = res;
                ResponseStatusCodeElapsedTimeTitle = FormatSuccessfulResponseTitle(res.ElapsedTime, (HttpStatusCode)res.StatusCode!);
                UpdateHeaders(res.Headers);
                ResponseRawContent = res.CanDisplayTextBody ? res.GetBodyAsText() : string.Format(Localizer.Instance["Response/BodyContentBinaryNotShown"], res.GetBodyAsBinary()!.Length);
                IsSaveResponseBodyToFileVisible = res.HasBody;
            }
            else if (res != null && !res.WasCancelled) // Not success, but also not cancelled. If cancelled, do nothing.
            {
                _res = res;
                ResponseStatusCodeElapsedTimeTitle = FormatFailedResponseTitle(res.ElapsedTime);
                UpdateHeaders(res.Headers);
                ResponseRawContent = res.Exception!.ToString();
                IsSaveResponseBodyToFileVisible = false;
            }
            else if (_res == null) // No response obtained yet.
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
                foreach (KeyValuePair<string, string> kvp in resHeaders)
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
            elapsedTime < _oneSecond ?
            string.Format(Localizer.Instance["Response/ElapsedTimeMillisecondsFormat"], (int)elapsedTime.TotalMilliseconds) :
            elapsedTime < _oneMinute ? // More or equal than one second, but less than one minute
            string.Format(Localizer.Instance["Response/ElapsedTimeSecondsFormat"], elapsedTime.TotalSeconds) : // TODO: Format digit separator according to language
            string.Format(Localizer.Instance["Response/ElapsedTimeMinutesFormat"], elapsedTime.Minutes, elapsedTime.Seconds);


    }
}
