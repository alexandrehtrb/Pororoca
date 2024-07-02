using System.Collections.ObjectModel;
using System.Reactive;
using AvaloniaEdit.Document;
using Pororoca.Desktop.Behaviors;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;
using Pororoca.Infrastructure.Features.Requester;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using static Pororoca.Domain.Features.Common.HttpVersionFormatter;
using static Pororoca.Domain.Features.Entities.Pororoca.Http.PororocaHttpRequestBody;

namespace Pororoca.Desktop.ViewModels;

public sealed class HttpRequestViewModel : CollectionOrganizationItemViewModel
{
    #region REQUEST

    private readonly PororocaRequester requester = PororocaRequester.Singleton;
    private readonly CollectionViewModel col;

    // To preserve the state of the last shown request tab
    [Reactive]
    public int RequestTabsSelectedIndex { get; set; }

    #region REQUEST HTTP METHOD
    public ObservableCollection<string> RequestMethodSelectionOptions { get; }

    [Reactive]
    public int RequestMethodSelectedIndex { get; set; }

    private HttpMethod RequestMethod =>
        AvailableHttpMethods[RequestMethodSelectedIndex];

    #endregion

    #region REQUEST URL

    private string requestUrlField;
    public string RequestUrl
    {
        get => this.requestUrlField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.requestUrlField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasRequestUrlValidationProblem)
                ClearInvalidRequestWarnings();
        }
    }

    [Reactive]
    public string ResolvedRequestUrlToolTip { get; set; }

    [Reactive]
    public bool HasRequestUrlValidationProblem { get; set; }

    #endregion

    #region REQUEST HTTP VERSION

    public ObservableCollection<string> RequestHttpVersionSelectionOptions { get; }

    private int requestHttpVersionSelectedIndexField;
    public int RequestHttpVersionSelectedIndex
    {
        get => this.requestHttpVersionSelectedIndexField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.requestHttpVersionSelectedIndexField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasRequestHttpVersionValidationProblem)
                ClearInvalidRequestWarnings();
        }
    }

    private decimal RequestHttpVersion =>
        AvailableHttpVersionsForHttp[RequestHttpVersionSelectedIndex];

    [Reactive]
    public bool HasRequestHttpVersionValidationProblem { get; set; }

    #endregion

    #region REQUEST VALIDATION MESSAGE

    private string? invalidRequestMessageErrorCode;

    [Reactive]
    public bool IsInvalidRequestMessageVisible { get; set; }

    [Reactive]
    public string? InvalidRequestMessage { get; set; }

    #endregion

    #region REQUEST HEADERS

    public RequestHeadersDataGridViewModel RequestHeadersTableVm { get; }

    #endregion

    #region REQUEST BODY

    [Reactive]
    public int RequestBodyModeSelectedIndex { get; set; }

    public PororocaHttpRequestBodyMode? RequestBodyMode =>
        HttpRequestBodyModeMapping.MapIndexToEnum(RequestBodyModeSelectedIndex);

    public static ObservableCollection<string> AllMimeTypes { get; } = new(MimeTypesDetector.AllMimeTypes);

    #region REQUEST BODY RAW

    private string? requestRawContentTypeField;
    public string? RequestRawContentType
    {
        get => this.requestRawContentTypeField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.requestRawContentTypeField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasRequestRawContentTypeValidationProblem)
                ClearInvalidRequestWarnings();
        }
    }

    [Reactive]
    public bool HasRequestRawContentTypeValidationProblem { get; set; }

    [Reactive]
    public TextDocument? RequestRawContentTextDocument { get; set; }

    public string? RequestRawContent
    {
        get => RequestRawContentTextDocument?.Text;
        set => RequestRawContentTextDocument = new(value ?? string.Empty);
    }

    #endregion

    #region REQUEST BODY FILE

    private string? requestFileContentTypeField;
    public string? RequestFileContentType
    {
        get => this.requestFileContentTypeField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.requestFileContentTypeField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasRequestFileContentTypeValidationProblem)
                ClearInvalidRequestWarnings();
        }
    }

    [Reactive]
    public bool HasRequestFileContentTypeValidationProblem { get; set; }

    private string? requestBodyFileSrcPathField;
    public string? RequestBodyFileSrcPath
    {
        get => this.requestBodyFileSrcPathField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.requestBodyFileSrcPathField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasRequestBodyFileSrcPathValidationProblem)
                ClearInvalidRequestWarnings();
        }
    }

    [Reactive]
    public bool HasRequestBodyFileSrcPathValidationProblem { get; set; }

    public ReactiveCommand<Unit, Unit> SearchRequestBodyRawFileCmd { get; }

    #endregion

    #region REQUEST BODY URL ENCODED

    public KeyValueParamsDataGridViewModel UrlEncodedParamsTableVm { get; }

    #endregion

    #region REQUEST BODY FORM DATA

    public FormDataParamsDataGridViewModel FormDataParamsTableVm { get; }

    #endregion

    #region REQUEST BODY GRAPHQL

    [Reactive]
    public string? RequestBodyGraphQlQuery { get; set; }

    [Reactive]
    public string? RequestBodyGraphQlVariables { get; set; }

    #endregion

    #endregion

    #region REQUEST AUTH

    [Reactive]
    public RequestAuthViewModel RequestAuthDataCtx { get; set; }

    #endregion

    #endregion

    #region SEND OR CANCEL REQUEST

    [Reactive]
    public string SendOrCancelRequestButtonText { get; set; }

    [Reactive]
    public string SendOrCancelRequestButtonToolTip { get; set; }

    private bool isRequestingField;
    public bool IsRequesting
    {
        get => this.isRequestingField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.isRequestingField, value);
            SendOrCancelRequestButtonText = value ?
                Localizer.Instance.HttpRequest.CancelRequest :
                Localizer.Instance.HttpRequest.SendRequest;
            SendOrCancelRequestButtonToolTip = value ?
                Localizer.Instance.HttpRequest.CancelRequestToolTip :
                Localizer.Instance.HttpRequest.SendRequestToolTip;
        }
    }

    private CancellationTokenSource? sendRequestCancellationTokenSourceField;

    [Reactive]
    public bool IsSendRequestProgressBarVisible { get; set; }

    #endregion

    #region RESPONSE

    [Reactive]
    public HttpResponseViewModel ResponseDataCtx { get; set; }

    #region RESPONSE CAPTURES

    public HttpResponseCapturesDataGridViewModel ResCapturesTableVm { get; }

    #endregion

    #endregion

    public HttpRequestViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                CollectionViewModel variableResolver,
                                PororocaHttpRequest req) : base(parentVm, req.Name)
    {
        #region COLLECTION ORGANIZATION
        Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);
        NameEditableVm.Icon = EditableTextBlockIcon.HttpRequest;
        #endregion

        #region REQUEST

        this.col = variableResolver;

        #endregion

        #region REQUEST HTTP METHOD, HTTP VERSION, URL AND HEADERS
        RequestMethodSelectionOptions = new(AvailableHttpMethods.Select(m => m.ToString()));
        int reqMethodSelectionIndex = RequestMethodSelectionOptions.IndexOf(req.HttpMethod);
        RequestMethodSelectedIndex = reqMethodSelectionIndex >= 0 ? reqMethodSelectionIndex : 0;

        ResolvedRequestUrlToolTip = this.requestUrlField = req.Url;

        RequestHttpVersionSelectionOptions = new(AvailableHttpVersionsForHttp.Select(FormatHttpVersion));
        int reqHttpVersionSelectionIndex = RequestHttpVersionSelectionOptions.IndexOf(FormatHttpVersion(req.HttpVersion));
        RequestHttpVersionSelectedIndex = reqHttpVersionSelectionIndex >= 0 ? reqHttpVersionSelectionIndex : 0;

        RequestHeadersTableVm = new(req.Headers);
        #endregion

        #region REQUEST BODY
        // TODO: Improve this, do not use fixed values to resolve index
        RequestBodyModeSelectedIndex = HttpRequestBodyModeMapping.MapEnumToIndex(req.Body?.Mode);
        // RAW
        RequestRawContentType = req.Body?.ContentType;
        RequestRawContent = req.Body?.RawContent;
        // FILE
        RequestFileContentType = req.Body?.ContentType;
        RequestBodyFileSrcPath = req.Body?.FileSrcPath;
        SearchRequestBodyRawFileCmd = ReactiveCommand.CreateFromTask(SearchRequestBodyRawFilePathAsync);
        // URL ENCODED
        UrlEncodedParamsTableVm = new(req.Body?.UrlEncodedValues);
        // FORM DATA
        FormDataParamsTableVm = new(req.Body?.FormDataValues);
        // GRAPHQL
        RequestBodyGraphQlQuery = req.Body?.GraphQlValues?.Query;
        RequestBodyGraphQlVariables = req.Body?.GraphQlValues?.Variables;
        #endregion

        #region REQUEST AUTH
        RequestAuthDataCtx = new(req.CustomAuth, true, ClearInvalidRequestWarnings);
        #endregion

        #region SEND OR CANCEL REQUEST
        SendOrCancelRequestButtonText = Localizer.Instance.HttpRequest.SendRequest;
        SendOrCancelRequestButtonToolTip = Localizer.Instance.HttpRequest.SendRequestToolTip;
        #endregion

        #region RESPONSE
        ResponseDataCtx = new(this.col);
        #region RESPONSE CAPTURES
        ResCapturesTableVm = new(req.ResponseCaptures);
        #endregion
        #endregion
    }

    #region COLLECTION ORGANIZATION

    protected override void CopyThis() =>
        ClipboardArea.Instance.PushToCopy(ToHttpRequest());

    private void OnLanguageChanged()
    {
        if (this.invalidRequestMessageErrorCode != null && IsInvalidRequestMessageVisible)
        {
            ShowInvalidRequestWarnings();
        }

        // this will trigger an update on the send/cancel req button texts
        bool isRequesting = this.isRequestingField;
        IsRequesting = isRequesting;
    }

    protected override void OnNameUpdated(string newName)
    {
        // IMPORTANT: always update list of http reqs paths after renaming HTTP request
        this.col.RemoveHttpRequestPathFromList(GetRequestPathInCollection());
        base.OnNameUpdated(newName);
        this.col.AddHttpRequestPathToList(GetRequestPathInCollection());
    }

    public override void DeleteThis()
    {
        base.DeleteThis();
        // IMPORTANT: always update list of http reqs paths after renaming HTTP request
        this.col.RemoveHttpRequestPathFromList(GetRequestPathInCollection());
    }

    public string GetRequestPathInCollection()
    {
        List<string> parts = new();

        CollectionOrganizationItemViewModel vm = this;
        do
        {
            // '/' needs to removed from the name to avoid problem when splitting path by '/'
            parts.Add(vm.Name.Replace("/", string.Empty));
        }
        while (vm.Parent is CollectionOrganizationItemViewModel parent && ((vm = parent) is not CollectionViewModel));

        parts.Reverse();
        return string.Join('/', parts);
    }

    #endregion

    #region REQUEST HTTP METHOD, HTTP VERSION AND URL

    public void UpdateResolvedRequestUrlToolTip()
    {
        var varResolver = ((IPororocaVariableResolver)this.col);
        ResolvedRequestUrlToolTip = IPororocaVariableResolver.ReplaceTemplates(RequestUrl, varResolver.GetEffectiveVariables());
    }

    #endregion

    #region REQUEST BODY FILE

    private async Task SearchRequestBodyRawFilePathAsync()
    {
        string? fileSrcPath = await SearchFileWithDialogAsync();
        if (fileSrcPath != null)
        {
            RequestBodyFileSrcPath = fileSrcPath;
            if (MimeTypesDetector.TryFindMimeTypeForFile(fileSrcPath, out string? foundMimeType))
            {
                RequestFileContentType = foundMimeType;
            }
            else
            {
                RequestFileContentType = MimeTypesDetector.DefaultMimeTypeForBinary;
            }
        }
    }

    #endregion

    #region CONVERT VIEW INPUTS TO REQUEST ENTITY

    private PororocaHttpRequestBody? WrapRequestBodyFromInputs() => RequestBodyMode switch
    {
        PororocaHttpRequestBodyMode.GraphQl => MakeGraphQlContent(RequestBodyGraphQlQuery, RequestBodyGraphQlVariables),
        PororocaHttpRequestBodyMode.FormData => MakeFormDataContent(FormDataParamsTableVm.Items.Select(p => p.ToFormDataParam())),
        PororocaHttpRequestBodyMode.UrlEncoded => MakeUrlEncodedContent(UrlEncodedParamsTableVm.Items.Select(p => p.ToKeyValueParam())),
        PororocaHttpRequestBodyMode.File => MakeFileContent(RequestBodyFileSrcPath ?? string.Empty, RequestFileContentType ?? string.Empty),
        PororocaHttpRequestBodyMode.Raw => MakeRawContent(RequestRawContent ?? string.Empty, RequestRawContentType ?? string.Empty),
        _ => null,
    };

    public PororocaHttpRequest ToHttpRequest() => new(
        Name: Name,
        HttpVersion: RequestHttpVersion,
        HttpMethod: RequestMethod.ToString(),
        Url: RequestUrl,
        CustomAuth: RequestAuthDataCtx.ToCustomAuth(),
        Headers: RequestHeadersTableVm.Items.Count == 0 ? null : RequestHeadersTableVm.ConvertItemsToDomain(),
        Body: WrapRequestBodyFromInputs(),
        ResponseCaptures: ResCapturesTableVm.Items.Count == 0 ? null : ResCapturesTableVm.ConvertItemsToDomain());

    #endregion

    #region SEND OR CANCEL REQUEST

    public Task SendOrCancelRequestAsync()
    {
        if (IsRequesting)
        {
            CancelRequest();
            return Task.CompletedTask;
        }
        else
        {
            return SendRequestAsync();
        }
    }

    public void CancelRequest() =>
        // CancellationToken will cause a failed PororocaResponse,
        // therefore is not necessary to here call method ShowNotSendingRequestUI()
        this.sendRequestCancellationTokenSourceField!.Cancel();

    public async Task SendRequestAsync()
    {
        ClearInvalidRequestWarnings();
        var generatedReq = ToHttpRequest();
        var effectiveVars = ((IPororocaVariableResolver)this.col).GetEffectiveVariables();
        if (!this.requester.IsValidRequest(effectiveVars, this.col.CollectionScopedAuth, generatedReq, out string? errorCode))
        {
            this.invalidRequestMessageErrorCode = errorCode;
            ShowInvalidRequestWarnings();
        }
        else
        {
            ShowSendingRequestUI();
            var res = await SendRequestAsync(generatedReq, effectiveVars);
            ResponseDataCtx.UpdateWithResponse(Name, res, ResCapturesTableVm.Items);
            ShowNotSendingRequestUI();
        }
    }

    private void ClearInvalidRequestWarnings()
    {
        this.invalidRequestMessageErrorCode = null;
        IsInvalidRequestMessageVisible = false;

        HasRequestUrlValidationProblem = false;
        HasRequestHttpVersionValidationProblem = false;
        HasRequestRawContentTypeValidationProblem = false;
        HasRequestFileContentTypeValidationProblem = false;
        HasRequestBodyFileSrcPathValidationProblem = false;

        RequestAuthDataCtx.ClearRequestAuthValidationWarnings();
    }

    private void ShowInvalidRequestWarnings()
    {
        string? errorCode = this.invalidRequestMessageErrorCode;
        InvalidRequestMessage = errorCode switch
        {
            TranslateRequestErrors.ClientCertificatePkcs12CertificateFileNotFound => Localizer.Instance.RequestValidation.ClientCertificateFileNotFound,
            TranslateRequestErrors.ClientCertificatePemCertificateFileNotFound => Localizer.Instance.RequestValidation.ClientCertificateFileNotFound,
            TranslateRequestErrors.ClientCertificatePkcs12PasswordCannotBeBlank => Localizer.Instance.RequestValidation.ClientCertificatePkcs12PasswordCannotBeBlank,
            TranslateRequestErrors.ClientCertificatePemPrivateKeyFileNotFound => Localizer.Instance.RequestValidation.ClientCertificatePemPrivateKeyFileNotFound,
            TranslateRequestErrors.ContentTypeCannotBeBlankReqBodyRaw => Localizer.Instance.RequestValidation.ContentTypeCannotBeBlankReqBodyRawOrFile,
            TranslateRequestErrors.ContentTypeCannotBeBlankReqBodyFile => Localizer.Instance.RequestValidation.ContentTypeCannotBeBlankReqBodyRawOrFile,
            TranslateRequestErrors.Http2UnavailableInOSVersion => Localizer.Instance.RequestValidation.Http2Unavailable,
            TranslateRequestErrors.Http3UnavailableInOSVersion => Localizer.Instance.RequestValidation.Http3Unavailable,
            TranslateRequestErrors.InvalidContentTypeFormData => Localizer.Instance.RequestValidation.InvalidContentTypeFormData,
            TranslateRequestErrors.InvalidContentTypeRaw => Localizer.Instance.RequestValidation.InvalidContentTypeRawOrFile,
            TranslateRequestErrors.InvalidContentTypeFile => Localizer.Instance.RequestValidation.InvalidContentTypeRawOrFile,
            TranslateRequestErrors.InvalidUrl => Localizer.Instance.RequestValidation.InvalidUrl,
            TranslateRequestErrors.ReqBodyFileNotFound => Localizer.Instance.RequestValidation.ReqBodyFileNotFound,
            TranslateRequestErrors.WindowsAuthLoginCannotBeBlank => Localizer.Instance.RequestValidation.WindowsAuthLoginCannotBeBlank,
            TranslateRequestErrors.WindowsAuthPasswordCannotBeBlank => Localizer.Instance.RequestValidation.WindowsAuthPasswordCannotBeBlank,
            TranslateRequestErrors.WindowsAuthDomainCannotBeBlank => Localizer.Instance.RequestValidation.WindowsAuthDomainCannotBeBlank,
            _ => Localizer.Instance.RequestValidation.InvalidUnknownCause
        };
        IsInvalidRequestMessageVisible = true;

        HasRequestUrlValidationProblem = (errorCode == TranslateRequestErrors.InvalidUrl);
        HasRequestHttpVersionValidationProblem =
        (errorCode == TranslateRequestErrors.Http2UnavailableInOSVersion || errorCode == TranslateRequestErrors.Http3UnavailableInOSVersion);
        HasRequestRawContentTypeValidationProblem =
        (errorCode == TranslateRequestErrors.ContentTypeCannotBeBlankReqBodyRaw || errorCode == TranslateRequestErrors.InvalidContentTypeRaw);
        HasRequestFileContentTypeValidationProblem =
        (errorCode == TranslateRequestErrors.ContentTypeCannotBeBlankReqBodyFile || errorCode == TranslateRequestErrors.InvalidContentTypeFile);
        HasRequestBodyFileSrcPathValidationProblem = (errorCode == TranslateRequestErrors.ReqBodyFileNotFound);

        if (HasRequestRawContentTypeValidationProblem
         || HasRequestFileContentTypeValidationProblem
         || HasRequestBodyFileSrcPathValidationProblem)
        {
            // TODO: Improve this, do not use fixed values to resolve index
            RequestTabsSelectedIndex = 1;
        }

        // do not colourize errors and switch tab if problem is in collection-scoped auth
        if (RequestAuthDataCtx.AuthMode == PororocaRequestAuthMode.InheritFromCollection)
            return;

        RequestAuthDataCtx.HighlightValidationProblems(errorCode);

        if (RequestAuthDataCtx.HasValidationProblem)
        {
            // TODO: Improve this, do not use fixed values to resolve index
            RequestTabsSelectedIndex = 2;
        }
    }

    private void ShowSendingRequestUI()
    {
        IsRequesting = true;
        IsSendRequestProgressBarVisible = true;
    }

    private void ShowNotSendingRequestUI()
    {
        IsRequesting = false;
        IsSendRequestProgressBarVisible = false;
    }

    private Task<PororocaHttpResponse> SendRequestAsync(PororocaHttpRequest generatedReq, IEnumerable<PororocaVariable> effectiveVars)
    {
        this.sendRequestCancellationTokenSourceField = new();
        // This needs to be done in a different thread.
        // Awaiting the request.RequestAsync() here, or simply returning its Task,
        // causes the UI to freeze for a few seconds, especially when performing the first request to a server.
        // That is why we are invoking the code to run in a new thread, like below.
        return Task.Run(async () => await this.requester.RequestAsync(effectiveVars, this.col.CollectionScopedAuth, this.col.CollectionScopedRequestHeaders, generatedReq, this.sendRequestCancellationTokenSourceField.Token));
    }

    #endregion

    #region OTHERS

    private static Task<string?> SearchFileWithDialogAsync() =>
        FileExporterImporter.SelectFileFromStorageAsync();

    #endregion
}