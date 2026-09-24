using System.Collections.ObjectModel;
using System.Diagnostics;
using ReactiveUI.Primitives;
using System.Threading.Channels;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using Pororoca.Desktop.Controls;
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
using ReactiveUI.SourceGenerators;
using static Pororoca.Desktop.Localization.TimeTextFormatter;
using static Pororoca.Domain.Features.Common.MimeTypesDetector;
using static Pororoca.Domain.Features.RequestRepeater.HttpRepeater;
using static Pororoca.Domain.Features.RequestRepeater.HttpRepetitionReporter;
using static Pororoca.Domain.Features.RequestRepeater.HttpRepetitionValidator;

namespace Pororoca.Desktop.ViewModels;

public sealed partial class HttpRepeaterViewModel : CollectionOrganizationItemViewModel
{
    #region COLLECTION ORGANIZATION

    private readonly PororocaRequester requester = PororocaRequester.Singleton;
    internal CollectionViewModel Collection { get; }
    internal PororocaVariableSyntaxHighlightingDefinitionSet PororocaVarSyntaxHighlightingDefinitionSet { get; }

    #endregion

    #region REPETITION CONFIG

    public ObservableCollection<string> CollectionHttpRequestsPaths { get; }

    public string? BaseRequestPath
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvalidRepetitionErrorCode = null;
        }
    }

    public ReactiveCommand<RxVoid, RxVoid> RefreshBaseRequestsListCmd { get; set; }

    // TODO: Don't materialize PororocaCollection, instead, extract BaseRequest
    // by searching ViewModels
    private PororocaHttpRequest? BaseRequest =>
        Collection.ToCollection().GetHttpRequestByPath(BaseRequestPath);

    [Reactive]
    public partial string? RepetitionStatusText { get; set; }

    [Reactive]
    public partial int RepetitionTabSelectedIndex { get; set; }

    [Reactive]
    public partial int RepetitionModeSelectedIndex { get; set; }

    public PororocaRepetitionMode RepetitionMode =>
        RepetitionModeMapping.MapIndexToEnum(RepetitionModeSelectedIndex);

    #region REQUEST VALIDATION MESSAGE

    [Reactive]
    public partial bool IsInvalidRepetitionErrorVisible { get; set; }

    [Reactive]
    public partial string? InvalidRepetitionError { get; set; }

    [Reactive]
    public partial bool HasBaseHttpRequestValidationProblem { get; set; }

    [Reactive]
    public partial bool HasDelayValidationProblem { get; set; }

    [Reactive]
    public partial bool HasNumberOfRepetitionsValidationProblem { get; set; }

    [Reactive]
    public partial bool HasMaximumRateValidationProblem { get; set; }

    [Reactive]
    public partial bool HasMaxDopValidationProblem { get; set; }

    [Reactive]
    public partial bool HasInputDataFileSrcPathValidationProblem { get; set; }

    private string? InvalidRepetitionErrorCode
    {
        get;
        set
        {
            field = value;
            IsInvalidRepetitionErrorVisible = value is not null;
            InvalidRepetitionError = value switch
            {
                TranslateRepetitionErrors.BaseHttpRequestNotSelected => Localizer.Instance.RequestValidation.RepetitionBaseHttpRequestNotSelected,
                TranslateRepetitionErrors.BaseHttpRequestNotFound => Localizer.Instance.RequestValidation.RepetitionBaseHttpRequestNotFound,
                TranslateRepetitionErrors.DelayCantBeNegative => Localizer.Instance.RequestValidation.RepetitionDelayCantBeNegative,
                TranslateRepetitionErrors.MaximumRateCantBeNegative => Localizer.Instance.RequestValidation.RepetitionMaximumRateCantBeNegative,
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
            HasMaximumRateValidationProblem = value == TranslateRepetitionErrors.MaximumRateCantBeNegative;
            HasNumberOfRepetitionsValidationProblem = value == TranslateRepetitionErrors.NumberOfRepetitionsMustBeAtLeast1;
            HasMaxDopValidationProblem = value == TranslateRepetitionErrors.MaxDopMustBeAtLeast1;

            HasInputDataFileSrcPathValidationProblem = (value == TranslateRepetitionErrors.InputDataFileNotFound);

            // TODO: Improve this, do not use fixed values to resolve index
            RepetitionTabSelectedIndex = value switch
            {
                TranslateRepetitionErrors.NumberOfRepetitionsMustBeAtLeast1 or
                TranslateRepetitionErrors.DelayCantBeNegative or
                TranslateRepetitionErrors.MaximumRateCantBeNegative or
                TranslateRepetitionErrors.MaxDopMustBeAtLeast1 => 0,

                TranslateRepetitionErrors.InputDataFileNotFound or
                TranslateRepetitionErrors.InputDataInvalid => 1,

                _ => RepetitionTabSelectedIndex
            };
        }
    }

    #endregion

    public int NumberOfRepetitionsToExecute
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvalidRepetitionErrorCode = null;
            const int minigunThreshold = 600;
            NameEditableVm.Icon = value switch
            {
                >= minigunThreshold => EditableTextBlockIcon.HttpMinigun,
                _ => EditableTextBlockIcon.HttpRepeater
            };
        }
    }

    public int MaximumRate
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvalidRepetitionErrorCode = null;
        }
    }

    public int MaxDop
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvalidRepetitionErrorCode = null;
        }
    }

    public int DelayInMs
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvalidRepetitionErrorCode = null;
        }
    }

    [Reactive]
    public partial bool RunInBackground { get; set; }

    [Reactive]
    public partial int InputDataTypeSelectedIndex { get; set; }

    public PororocaRepetitionInputDataType? InputDataType =>
        RepetitionInputDataTypeMapping.MapIndexToEnum(InputDataTypeSelectedIndex);

    [Reactive]
    public partial TextDocument? InputDataRawTextDocument { get; set; }

    public string? InputDataRawText
    {
        get => InputDataRawTextDocument?.Text;
        set => InputDataRawTextDocument = new(value ?? string.Empty);
    }

    public string? InputDataFileSrcPath
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvalidRepetitionErrorCode = null;
        }
    }


    public ReactiveCommand<RxVoid, RxVoid> SearchInputDataFileCmd { get; set; }

    #endregion

    #region REPETITION RESULTS

    private string? nameOfEnvironmentUsed;

    private string? nameOfBaseHttpRequestUsed;

    private CancellationTokenSource? cancellationTokenSource;

    [Reactive]
    public partial string StartOrStopRepetitionButtonText { get; set; }

    [Reactive]
    public partial string StartOrStopRepetitionButtonToolTip { get; set; }

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
    public partial bool HasFinishedRepetition { get; set; }

    [Reactive]
    public partial bool ShowRepetitionSuccessfulTip { get; set; }

    [Reactive]
    public partial ObservableCollection<HttpRepetitionResultViewModel> RepetitionResults { get; set; }

    [Reactive]
    public partial int NumberOfRepetitionsExecuted { get; set; }

    private int NumberOfRepetitionsSuccessful { get; set; }

    public HttpRepetitionResultViewModel? SelectedRepetitionResult
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
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
    public partial ReactiveCommand<RxVoid, RxVoid> ExportReportCmd { get; set; }

    [Reactive]
    public partial ReactiveCommand<RxVoid, RxVoid> SaveAllResponsesCmd { get; set; }

    [Reactive]
    public partial ReactiveCommand<RxVoid, RxVoid> ExportAllLogsCmd { get; set; }

    #endregion

    #region REPETITION RESULT DETAILS

    [Reactive]
    public partial HttpResponseViewModel ResponseDataCtx { get; set; }

    [Reactive]
    public partial VariablesDataGridViewModel InputLineTableVm { get; set; }

    #endregion

    public HttpRepeaterViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                 CollectionViewModel col,
                                 PororocaHttpRepetition rep) : base(parentVm, rep.Name)
    {
        #region COLLECTION ORGANIZATION
        Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);
        Collection = col;
        PororocaVarSyntaxHighlightingDefinitionSet = Collection.PororocaVarSyntaxHighlightingDefinitionSet;
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
        InputLineTableVm = new(Collection);
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
        MaxRatePerSecond: MaximumRate == 0 ? null : MaximumRate,
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