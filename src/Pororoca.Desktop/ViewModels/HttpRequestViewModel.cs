using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;
using AvaloniaEdit.Document;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;
using Pororoca.Infrastructure.Features.Requester;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;

namespace Pororoca.Desktop.ViewModels;

public sealed class HttpRequestViewModel : CollectionOrganizationItemViewModel
{
    #region REQUEST

    private readonly IPororocaRequester requester = PororocaRequester.Singleton;
    private readonly CollectionViewModel variableResolver;

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
            if (HasRequestUrlValidationProblem) ClearInvalidRequestWarnings();
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
            if (HasRequestHttpVersionValidationProblem) ClearInvalidRequestWarnings();
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

    public KeyValueParamsDataGridViewModel RequestHeadersTableVm { get; }

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
            if (HasRequestRawContentTypeValidationProblem) ClearInvalidRequestWarnings();
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
            if (HasRequestFileContentTypeValidationProblem) ClearInvalidRequestWarnings();
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
            if (HasRequestBodyFileSrcPathValidationProblem) ClearInvalidRequestWarnings();
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
    public bool IsRequesting { get; set; }

    private CancellationTokenSource? sendRequestCancellationTokenSourceField;

    public ReactiveCommand<Unit, Unit> CancelRequestCmd { get; }
    public ReactiveCommand<Unit, Unit> SendRequestCmd { get; }

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
        NameEditableVm.IsHttpRequest = true;
        #endregion

        #region REQUEST

        this.variableResolver = variableResolver;

        #endregion

        #region REQUEST HTTP METHOD, HTTP VERSION, URL AND HEADERS
        RequestMethodSelectionOptions = new(AvailableHttpMethods.Select(m => m.ToString()));
        int reqMethodSelectionIndex = RequestMethodSelectionOptions.IndexOf(req.HttpMethod);
        RequestMethodSelectedIndex = reqMethodSelectionIndex >= 0 ? reqMethodSelectionIndex : 0;

        ResolvedRequestUrlToolTip = this.requestUrlField = req.Url;

        RequestHttpVersionSelectionOptions = new(AvailableHttpVersionsForHttp.Select(FormatHttpVersionString));
        int reqHttpVersionSelectionIndex = RequestHttpVersionSelectionOptions.IndexOf(FormatHttpVersionString(req.HttpVersion));
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
        RequestAuthDataCtx = new(req.CustomAuth, true, this.ClearInvalidRequestWarnings);
        #endregion

        #region SEND OR CANCEL REQUEST
        CancelRequestCmd = ReactiveCommand.Create(CancelRequest);
        SendRequestCmd = ReactiveCommand.CreateFromTask(SendRequestAsync);
        #endregion

        #region RESPONSE
        ResponseDataCtx = new(this.variableResolver, this);
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
    }

    #endregion

    #region REQUEST HTTP METHOD, HTTP VERSION AND URL

    public void UpdateResolvedRequestUrlToolTip() =>
        ResolvedRequestUrlToolTip = ((IPororocaVariableResolver)this.variableResolver).ReplaceTemplates(RequestUrl);

    private static string FormatHttpVersionString(decimal httpVersion) =>
        httpVersion switch
        {
            1.0m => "HTTP/1.0",
            1.1m => "HTTP/1.1",
            2.0m => "HTTP/2",
            3.0m => "HTTP/3",
            _ => string.Format(CultureInfo.InvariantCulture, "HTTP/{0:0.0}", httpVersion)
        };

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

    private PororocaHttpRequestBody? WrapRequestBodyFromInputs()
    {
        PororocaHttpRequestBody body = new();
        switch (RequestBodyMode)
        {
            case PororocaHttpRequestBodyMode.GraphQl:
                body.SetGraphQlContent(RequestBodyGraphQlQuery, RequestBodyGraphQlVariables);
                break;
            case PororocaHttpRequestBodyMode.FormData:
                body.SetFormDataContent(FormDataParamsTableVm.Items.Select(p => p.ToFormDataParam()));
                break;
            case PororocaHttpRequestBodyMode.UrlEncoded:
                body.SetUrlEncodedContent(UrlEncodedParamsTableVm.Items.Select(p => p.ToKeyValueParam()));
                break;
            case PororocaHttpRequestBodyMode.File:
                body.SetFileContent(RequestBodyFileSrcPath ?? string.Empty, RequestFileContentType ?? string.Empty);
                break;
            case PororocaHttpRequestBodyMode.Raw:
                body.SetRawContent(RequestRawContent ?? string.Empty, RequestRawContentType ?? string.Empty);
                break;
            default:
                return null;
        }
        return body;
    }

    public PororocaHttpRequest ToHttpRequest()
    {
        PororocaHttpRequest newReq = new(Name);
        UpdateRequestWithInputs(newReq);
        return newReq;
    }

    private void UpdateRequestWithInputs(PororocaHttpRequest request) =>
        request.Update(
            name: Name,
            httpVersion: RequestHttpVersion,
            httpMethod: RequestMethod.ToString(),
            url: RequestUrl,
            customAuth: RequestAuthDataCtx.ToCustomAuth(),
            headers: RequestHeadersTableVm.Items.Count == 0 ? null : RequestHeadersTableVm.ConvertItemsToDomain(),
            body: WrapRequestBodyFromInputs(),
            captures: ResCapturesTableVm.Items.Count == 0 ? null : ResCapturesTableVm.ConvertItemsToDomain());

    #endregion

    #region SEND OR CANCEL REQUEST

    public void CancelRequest() =>
        // CancellationToken will cause a failed PororocaResponse,
        // therefore is not necessary to here call method ShowNotSendingRequestUI()
        this.sendRequestCancellationTokenSourceField!.Cancel();

    public async Task SendRequestAsync()
    {
        ClearInvalidRequestWarnings();
        var generatedReq = ToHttpRequest();
        if (!this.requester.IsValidRequest(this.variableResolver, generatedReq, out string? errorCode))
        {
            this.invalidRequestMessageErrorCode = errorCode;
            ShowInvalidRequestWarnings();
        }
        else
        {
            ShowSendingRequestUI();
            var res = await SendRequestAsync(generatedReq);
            ResponseDataCtx.UpdateWithResponse(res);
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

        RequestAuthDataCtx.HasWindowsAuthLoginProblem = errorCode == TranslateRequestErrors.WindowsAuthLoginCannotBeBlank;
        RequestAuthDataCtx.HasWindowsAuthPasswordProblem = errorCode == TranslateRequestErrors.WindowsAuthPasswordCannotBeBlank;
        RequestAuthDataCtx.HasWindowsAuthDomainProblem = errorCode == TranslateRequestErrors.WindowsAuthDomainCannotBeBlank;

        RequestAuthDataCtx.HasClientCertificateAuthPkcs12CertificateFilePathProblem = 
        (errorCode == TranslateRequestErrors.ClientCertificatePkcs12CertificateFileNotFound);
        RequestAuthDataCtx.HasClientCertificateAuthPkcs12FilePasswordProblem =
        (errorCode == TranslateRequestErrors.ClientCertificatePkcs12PasswordCannotBeBlank);
        RequestAuthDataCtx.HasClientCertificateAuthPemCertificateFilePathProblem =
        (errorCode == TranslateRequestErrors.ClientCertificatePemCertificateFileNotFound);
        RequestAuthDataCtx.HasClientCertificateAuthPemPrivateKeyFilePathProblem =
        (errorCode == TranslateRequestErrors.ClientCertificatePemPrivateKeyFileNotFound);

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

    private Task<PororocaHttpResponse> SendRequestAsync(PororocaHttpRequest generatedReq)
    {
        bool disableSslVerification = ((MainWindowViewModel)MainWindow.Instance!.DataContext!).IsSslVerificationDisabled;
        this.sendRequestCancellationTokenSourceField = new();
        // This needs to be done in a different thread.
        // Awaiting the request.RequestAsync() here, or simply returning its Task,
        // causes the UI to freeze for a few seconds, especially when performing the first request to a server.
        // That is why we are invoking the code to run in a new thread, like below.
        return Task.Run(async () => await this.requester.RequestAsync(this.variableResolver, generatedReq, disableSslVerification, this.sendRequestCancellationTokenSourceField.Token));
    }

    #endregion

    #region OTHERS

    private static Task<string?> SearchFileWithDialogAsync() =>
        FileExporterImporter.SelectFileFromStorageAsync();

    #endregion
}