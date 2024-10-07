using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Channels;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
using Pororoca.Domain.Features.ExportLog;
using Pororoca.Domain.Features.RequestRepeater;
using Pororoca.Domain.Features.VariableResolution;
using Pororoca.Infrastructure.Features.Requester;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static Pororoca.Desktop.Localization.TimeTextFormatter;
using static Pororoca.Domain.Features.Common.MimeTypesDetector;
using static Pororoca.Domain.Features.RequestRepeater.HttpRepeater;
using static Pororoca.Domain.Features.RequestRepeater.HttpRepetitionReporter;
using static Pororoca.Domain.Features.RequestRepeater.HttpRepetitionValidator;

namespace Pororoca.Desktop.ViewModels;

public sealed class HttpRepeaterViewModel : CollectionOrganizationItemViewModel
{
    #region COLLECTION ORGANIZATION

    private readonly PororocaRequester requester = PororocaRequester.Singleton;
    internal CollectionViewModel Collection { get; }

    #endregion

    #region REPETITION CONFIG

    public ObservableCollection<string> CollectionHttpRequestsPaths { get; }

    private string? baseRequestPathField;
    public string? BaseRequestPath
    {
        get => this.baseRequestPathField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.baseRequestPathField, value);
            InvalidRepetitionErrorCode = null;
        }
    }

    public ReactiveCommand<Unit, Unit> RefreshBaseRequestsListCmd { get; set; }

    // TODO: Don't materialize PororocaCollection, instead, extract BaseRequest
    // by searching ViewModels
    private PororocaHttpRequest? BaseRequest =>
        Collection.ToCollection().GetHttpRequestByPath(BaseRequestPath);

    [Reactive]
    public string? RepetitionStatusText { get; set; }

    [Reactive]
    public int RepetitionTabSelectedIndex { get; set; }

    [Reactive]
    public int RepetitionModeSelectedIndex { get; set; }

    public PororocaRepetitionMode RepetitionMode =>
        RepetitionModeMapping.MapIndexToEnum(RepetitionModeSelectedIndex);

    #region REQUEST VALIDATION MESSAGE

    [Reactive]
    public bool IsInvalidRepetitionErrorVisible { get; set; }

    [Reactive]
    public string? InvalidRepetitionError { get; set; }

    [Reactive]
    public bool HasBaseHttpRequestValidationProblem { get; set; }

    [Reactive]
    public bool HasDelayValidationProblem { get; set; }

    [Reactive]
    public bool HasNumberOfRepetitionsValidationProblem { get; set; }

    [Reactive]
    public bool HasMaxDopValidationProblem { get; set; }

    [Reactive]
    public bool HasInputDataFileSrcPathValidationProblem { get; set; }

    private string? invalidRepetitionErrorCodeField;
    private string? InvalidRepetitionErrorCode
    {
        get => this.invalidRepetitionErrorCodeField;
        set
        {
            this.invalidRepetitionErrorCodeField = value;
            IsInvalidRepetitionErrorVisible = value is not null;
            InvalidRepetitionError = value switch
            {
                TranslateRepetitionErrors.BaseHttpRequestNotSelected => Localizer.Instance.RequestValidation.RepetitionBaseHttpRequestNotSelected,
                TranslateRepetitionErrors.BaseHttpRequestNotFound => Localizer.Instance.RequestValidation.RepetitionBaseHttpRequestNotFound,
                TranslateRepetitionErrors.DelayCantBeNegative => Localizer.Instance.RequestValidation.RepetitionDelayCantBeNegative,
                TranslateRepetitionErrors.NumberOfRepetitionsMustBeAtLeast1 => Localizer.Instance.RequestValidation.RepetitionNumberOfRepetitionsMustBeAtLeast1,
                TranslateRepetitionErrors.MaxDopMustBeAtLeast1 => Localizer.Instance.RequestValidation.RepetitionMaxDopMustBeAtLeast1,
                TranslateRepetitionErrors.InputDataFileNotFound => Localizer.Instance.RequestValidation.RepetitionInputDataFileNotFound,
                TranslateRepetitionErrors.InputDataInvalid => Localizer.Instance.RequestValidation.RepetitionInputDataInvalid,
                TranslateRepetitionErrors.InputDataAtLeastOneLine => Localizer.Instance.RequestValidation.RepetitionInputDataAtLeastOneLine,
                null => string.Empty,
                _ => Localizer.Instance.RequestValidation.InvalidUnknownCause
            };
            HasBaseHttpRequestValidationProblem = (value == TranslateRepetitionErrors.BaseHttpRequestNotSelected
                                                 || value == TranslateRepetitionErrors.BaseHttpRequestNotFound);

            HasDelayValidationProblem = value == TranslateRepetitionErrors.DelayCantBeNegative;
            HasNumberOfRepetitionsValidationProblem = value == TranslateRepetitionErrors.NumberOfRepetitionsMustBeAtLeast1;
            HasMaxDopValidationProblem = value == TranslateRepetitionErrors.MaxDopMustBeAtLeast1;

            HasInputDataFileSrcPathValidationProblem = (value == TranslateRepetitionErrors.InputDataFileNotFound);

            // TODO: Improve this, do not use fixed values to resolve index
            RepetitionTabSelectedIndex = value switch
            {
                TranslateRepetitionErrors.NumberOfRepetitionsMustBeAtLeast1 or
                TranslateRepetitionErrors.DelayCantBeNegative or
                TranslateRepetitionErrors.MaxDopMustBeAtLeast1 => 0,

                TranslateRepetitionErrors.InputDataFileNotFound or
                TranslateRepetitionErrors.InputDataInvalid => 1,

                _ => RepetitionTabSelectedIndex
            };
        }
    }

    #endregion

    private int numberOfRepetitionsToExecuteField;
    public int NumberOfRepetitionsToExecute
    {
        get => this.numberOfRepetitionsToExecuteField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.numberOfRepetitionsToExecuteField, value);
            InvalidRepetitionErrorCode = null;
            const int minigunThreshold = 600;
            NameEditableVm.Icon = value switch
            {
                >= minigunThreshold => EditableTextBlockIcon.HttpMinigun,
                _ => EditableTextBlockIcon.HttpRepeater
            };
        }
    }

    private int maxDopField;
    public int MaxDop
    {
        get => this.maxDopField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.maxDopField, value);
            InvalidRepetitionErrorCode = null;
        }
    }

    private int delayInMsField;
    public int DelayInMs
    {
        get => this.delayInMsField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.delayInMsField, value);
            InvalidRepetitionErrorCode = null;
        }
    }

    [Reactive]
    public bool RunInBackground { get; set; }

    [Reactive]
    public int InputDataTypeSelectedIndex { get; set; }

    public PororocaRepetitionInputDataType? InputDataType =>
        RepetitionInputDataTypeMapping.MapIndexToEnum(InputDataTypeSelectedIndex);

    [Reactive]
    public TextDocument? InputDataRawTextDocument { get; set; }

    public string? InputDataRawText
    {
        get => InputDataRawTextDocument?.Text;
        set => InputDataRawTextDocument = new(value ?? string.Empty);
    }


    private string? inputDataFileSrcPathField;
    public string? InputDataFileSrcPath
    {
        get => this.inputDataFileSrcPathField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.inputDataFileSrcPathField, value);
            InvalidRepetitionErrorCode = null;
        }
    }


    public ReactiveCommand<Unit, Unit> SearchInputDataFileCmd { get; set; }

    #endregion

    #region REPETITION RESULTS

    private string? nameOfEnvironmentUsed;

    private string? nameOfBaseHttpRequestUsed;

    private CancellationTokenSource? cancellationTokenSource;

    [Reactive]
    public string StartOrStopRepetitionButtonText { get; set; }

    [Reactive]
    public string StartOrStopRepetitionButtonToolTip { get; set; }

    private bool isRepetitionRunningField;
    public bool IsRepetitionRunning
    {
        get => this.isRepetitionRunningField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.isRepetitionRunningField, value);
            StartOrStopRepetitionButtonText = value ?
                Localizer.Instance.HttpRepeater.Stop :
                Localizer.Instance.HttpRepeater.Start;
            StartOrStopRepetitionButtonToolTip = value ?
                Localizer.Instance.HttpRepeater.StopTip :
                Localizer.Instance.HttpRepeater.StartTip;
        }
    }

    [Reactive]
    public bool HasFinishedRepetition { get; set; }

    [Reactive]
    public bool ShowRepetitionSuccessfulTip { get; set; }

    [Reactive]
    public ObservableCollection<HttpRepetitionResultViewModel> RepetitionResults { get; set; }

    [Reactive]
    public int NumberOfRepetitionsExecuted { get; set; }

    private int NumberOfRepetitionsSuccessful { get; set; }

    private HttpRepetitionResultViewModel? selectedRepetitionResultField;
    public HttpRepetitionResultViewModel? SelectedRepetitionResult
    {
        get => this.selectedRepetitionResultField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.selectedRepetitionResultField, value);
            ResponseDataCtx.UpdateWithResponse(this.nameOfBaseHttpRequestUsed ?? "MyReq", value?.Result?.Response, null);
            InputLineTableVm.Items.Clear();
            if (value?.Result.InputLine is not null)
            {
                foreach (var pv in value.Result.InputLine)
                {
                    InputLineTableVm.Items.Add(new(InputLineTableVm.Items, pv));
                }
            }
        }
    }

    [Reactive]
    public ReactiveCommand<Unit, Unit> ExportReportCmd { get; set; }

    [Reactive]
    public ReactiveCommand<Unit, Unit> SaveAllResponsesCmd { get; set; }

    [Reactive]
    public ReactiveCommand<Unit, Unit> ExportAllLogsCmd { get; set; }

    #endregion

    #region REPETITION RESULT DETAILS

    [Reactive]
    public HttpResponseViewModel ResponseDataCtx { get; set; }

    [Reactive]
    public VariablesDataGridViewModel InputLineTableVm { get; set; }

    #endregion

    public HttpRepeaterViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                 CollectionViewModel col,
                                 PororocaHttpRepetition rep) : base(parentVm, rep.Name)
    {
        #region COLLECTION ORGANIZATION
        Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);
        Collection = col;
        #endregion

        #region REPETITION CONFIG
        CollectionHttpRequestsPaths = Collection.HttpRequestsPaths;
        BaseRequestPath = rep.BaseRequestPath;
        RefreshBaseRequestsListCmd = ReactiveCommand.Create(RefreshBaseRequestsList);
        RepetitionModeSelectedIndex = RepetitionModeMapping.MapEnumToIndex(rep.RepetitionMode);
        NumberOfRepetitionsToExecute = rep.NumberOfRepetitions ?? 1;
        MaxDop = rep.MaxDop ?? 1;
        DelayInMs = rep.DelayInMs ?? 0;
        InputDataTypeSelectedIndex = rep.InputData is null ? 0 : RepetitionInputDataTypeMapping.MapEnumToIndex(rep.InputData.Type);
        InputDataRawText = rep.InputData?.Type == PororocaRepetitionInputDataType.RawJsonArray ? rep.InputData.RawJsonArray : null;
        InputDataFileSrcPath = rep.InputData?.Type == PororocaRepetitionInputDataType.File ? rep.InputData.InputFilePath : null;
        SearchInputDataFileCmd = ReactiveCommand.CreateFromTask(SearchInputDataFileAsync);
        #endregion

        #region REPETITION RESULTS
        RepetitionResults = [];
        StartOrStopRepetitionButtonText = Localizer.Instance.HttpRepeater.Start;
        StartOrStopRepetitionButtonToolTip = Localizer.Instance.HttpRepeater.StartTip;
        ExportReportCmd = ReactiveCommand.CreateFromTask(ExportReportAsync);
        SaveAllResponsesCmd = ReactiveCommand.CreateFromTask(SaveAllResponsesAsync);
        ExportAllLogsCmd = ReactiveCommand.CreateFromTask(ExportAllLogsAsync);
        #endregion

        #region REPETITION RESULT DETAILS
        ResponseDataCtx = new(Collection);
        InputLineTableVm = new();
        #endregion
    }

    private void RefreshBaseRequestsList()
    {
        Collection.UpdateListOfHttpRequestsPaths();
        BaseRequestPath = null;
    }

    public Task StartOrStopRepetitionAsync()
    {
        if (IsRepetitionRunning)
        {
            StopRepetition();
            return Task.CompletedTask;
        }
        else
        {
            return StartRepetitionAsync();
        }
    }

    public void StopRepetition() =>
        this.cancellationTokenSource?.Cancel();

    public async Task StartRepetitionAsync()
    {
        var effectiveVars = ((IPororocaVariableResolver)Collection).GetEffectiveVariables();
        var (valid, errorCode, resolvedInputData) = await IsValidRepetitionAsync(effectiveVars, BaseRequest, ToHttpRepetition(), default);

        if (!valid)
        {
            ShowRepetitionSuccessfulTip = false;
            RepetitionStatusText = null;
            InvalidRepetitionErrorCode = errorCode;
        }
        else
        {
            InvalidRepetitionErrorCode = null;
            this.cancellationTokenSource = new();
            NumberOfRepetitionsExecuted = 0;
            NumberOfRepetitionsSuccessful = 0;
            NumberOfRepetitionsToExecute = RepetitionMode == PororocaRepetitionMode.Sequential ?
                                           resolvedInputData!.Length : NumberOfRepetitionsToExecute;
            IsRepetitionRunning = true;
            HasFinishedRepetition = false;
            ShowRepetitionSuccessfulTip = false;
            RepetitionStatusText = null;
            RepetitionResults.Clear();
            SelectedRepetitionResult = null;
            this.nameOfEnvironmentUsed = Collection.CurrentEnvironmentVm?.Name;
            this.nameOfBaseHttpRequestUsed = BaseRequest?.Name;

            var channelReader = StartRepetition(this.requester, effectiveVars, resolvedInputData, Collection.CollectionScopedAuth, Collection.CollectionScopedRequestHeaders, ToHttpRepetition(), BaseRequest!, this.cancellationTokenSource.Token);
            Dispatcher.UIThread.Post(async () => await CollectRepetitionResultsAsync(channelReader));
        }
    }

    private async Task CollectRepetitionResultsAsync(ChannelReader<PororocaHttpRepetitionResult> channelReader)
    {
        Stopwatch sw = new();
        sw.Start();
        await foreach (var result in channelReader.ReadAllAsync())
        {
            NumberOfRepetitionsExecuted++;
            if (result.HasHttp2xxStatusCode)
            {
                NumberOfRepetitionsSuccessful++;
            }

            HttpRepetitionResultViewModel vm = new(NumberOfRepetitionsExecuted, result);
            RepetitionResults.Add(vm);
            if (ShouldUpdateRepetitionStatusText(NumberOfRepetitionsToExecute, NumberOfRepetitionsExecuted))
            {
                ShowRepetitionSuccessfulTip = true;
                var estimatedTimeRemaining = EstimateRemainingTime(NumberOfRepetitionsToExecute, NumberOfRepetitionsExecuted, sw.Elapsed);
                RepetitionStatusText = string.Format(Localizer.Instance.HttpRepeater.RepetitionOngoingStatus, NumberOfRepetitionsExecuted, NumberOfRepetitionsToExecute, NumberOfRepetitionsSuccessful, FormatRemainingTimeText(estimatedTimeRemaining));
            }
        }
        sw.Stop();
        IsRepetitionRunning = false;
        HasFinishedRepetition = true;
        RepetitionStatusText = string.Format(Localizer.Instance.HttpRepeater.RepetitionFinishedStatus, NumberOfRepetitionsExecuted, NumberOfRepetitionsSuccessful, FormatTimeText(sw.Elapsed));
        NumberOfRepetitionsExecuted = 0;
    }

    private async Task ExportAllLogsAsync()
    {
        string? destinationFolderPath = await FileExporterImporter.SelectFolderAsync();
        if (destinationFolderPath is not null)
        {
            for (int i = 0; i < RepetitionResults.Count; i++)
            {
                var result = RepetitionResults[i].Result;
                string filePath = Path.Combine(destinationFolderPath, $"iteration{i + 1}.log");
                if (result.Response is not null)
                {
                    string logTxt = HttpLogExporter.ProduceHttpLog(result.Response);
                    await File.WriteAllTextAsync(filePath, logTxt).ConfigureAwait(false);
                }
            }
        }
    }

    private async Task SaveAllResponsesAsync()
    {
        string? destinationFolderPath = await FileExporterImporter.SelectFolderAsync();
        if (destinationFolderPath is not null)
        {
            for (int i = 0; i < RepetitionResults.Count; i++)
            {
                var result = RepetitionResults[i].Result;
                if (result.Response?.HasBody == true)
                {
                    string fileExt = TryFindFileExtensionForContentType(result.Response?.ContentType ?? string.Empty, out string? fileExtensionWithoutDot) ?
                                     ('.' + fileExtensionWithoutDot!) : string.Empty;

                    string filePath = Path.Combine(destinationFolderPath, $"iteration{i + 1}{fileExt}");

                    await File.WriteAllBytesAsync(filePath, result.Response!.GetBodyAsBinary()!).ConfigureAwait(false);
                }
            }
        }
    }

    private async Task ExportReportAsync()
    {
        if (RepetitionResults.Any())
        {
            var firstRequestStartedAt = RepetitionResults[0].Result?.Response?.StartedAtUtc ?? DateTimeOffset.Now;
            string env = this.nameOfEnvironmentUsed is not null ? $"_{this.nameOfEnvironmentUsed}" : string.Empty;
            string reportInitialFileName = $"report_{Name}{env}_{firstRequestStartedAt:yyyyMMdd-HHmmss}.csv";
            string? destinationFilePath = await FileExporterImporter.SelectPathForFileToBeSavedAsync(reportInitialFileName);
            if (destinationFilePath is not null)
            {
                await Task.Run(async () => await WriteReportAsync(RepetitionResults.Select(x => x.Result), destinationFilePath).ConfigureAwait(false));
            }
        }
    }

    internal PororocaHttpRepetition ToHttpRepetition() => new(
        Name: Name,
        BaseRequestPath: BaseRequestPath ?? string.Empty,
        RepetitionMode: RepetitionMode,
        NumberOfRepetitions: RepetitionMode == PororocaRepetitionMode.Sequential ? null : NumberOfRepetitionsToExecute,
        MaxDop: RepetitionMode == PororocaRepetitionMode.Sequential ? null : MaxDop,
        DelayInMs: DelayInMs == 0 ? null : DelayInMs,
        InputData: RepetitionMode == PororocaRepetitionMode.Simple ? null : new((PororocaRepetitionInputDataType)InputDataType!, InputDataRawText, InputDataFileSrcPath));

    private void OnLanguageChanged()
    {
        if (InvalidRepetitionErrorCode is not null)
        {
            string code = InvalidRepetitionErrorCode;
            InvalidRepetitionErrorCode = code; // this will trigger an update
        }

        // this will trigger an update on the start/stop rep button texts
        bool isRepRunning = this.isRepetitionRunningField;
        IsRepetitionRunning = isRepRunning;
    }

    private async Task SearchInputDataFileAsync()
    {
        string? fileSrcPath = await FileExporterImporter.SelectFileFromStorageAsync();
        if (fileSrcPath != null)
        {
            InputDataFileSrcPath = fileSrcPath;
        }
    }

    private static bool ShouldUpdateRepetitionStatusText(int total, int executed)
    {
        if (total <= 100 && executed <= 100)
            return true;

        int samplingRate = total switch
        {
            >= 10000 => 200,
            >= 1000 => 100,
            > 100 => 10,
            _ => 1
        };

        return executed % samplingRate == 0;
    }
}