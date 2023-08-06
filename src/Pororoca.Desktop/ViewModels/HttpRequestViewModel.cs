using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;
using AvaloniaEdit.Document;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;
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
    private readonly IPororocaVariableResolver variableResolver;

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

    public ObservableCollection<KeyValueParamViewModel> RequestHeaders { get; }
    public ReactiveCommand<Unit, Unit> AddNewRequestHeaderCmd { get; }

    #endregion

    #region REQUEST BODY

    [Reactive]
    public int RequestBodyModeSelectedIndex { get; set; }

    public PororocaHttpRequestBodyMode? RequestBodyMode => RequestBodyModeSelectedIndex switch
    {
        0 => null,
        1 => PororocaHttpRequestBodyMode.Raw,
        2 => PororocaHttpRequestBodyMode.File,
        3 => PororocaHttpRequestBodyMode.UrlEncoded,
        4 => PororocaHttpRequestBodyMode.FormData,
        5 => PororocaHttpRequestBodyMode.GraphQl,
        _ => null
    };

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

    public ObservableCollection<KeyValueParamViewModel> UrlEncodedParams { get; }
    public ReactiveCommand<Unit, Unit> AddNewUrlEncodedParamCmd { get; }

    #endregion

    #region REQUEST BODY FORM DATA

    public ObservableCollection<HttpRequestFormDataParamViewModel> FormDataParams { get; }
    public ReactiveCommand<Unit, Unit> AddNewFormDataTextParamCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewFormDataFileParamCmd { get; }

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

    #endregion

    public HttpRequestViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                IPororocaVariableResolver variableResolver,
                                PororocaHttpRequest req) : base(parentVm, req.Name)
    {
        #region COLLECTION ORGANIZATION
        Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);
        NameEditableTextBlockViewDataCtx.IsHttpRequest = true;
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

        RequestHeaders = new();
        if (req.Headers is not null)
        {
            foreach (var h in req.Headers)
            {
                RequestHeaders.Add(new(RequestHeaders, h));
            }
        }
        AddNewRequestHeaderCmd = ReactiveCommand.Create(AddNewHeader);
        #endregion

        #region REQUEST BODY
        // TODO: Improve this, do not use fixed values to resolve index
        switch (req.Body?.Mode)
        {
            case PororocaHttpRequestBodyMode.GraphQl:
                RequestBodyModeSelectedIndex = 5;
                break;
            case PororocaHttpRequestBodyMode.FormData:
                RequestBodyModeSelectedIndex = 4;
                break;
            case PororocaHttpRequestBodyMode.UrlEncoded:
                RequestBodyModeSelectedIndex = 3;
                break;
            case PororocaHttpRequestBodyMode.File:
                RequestBodyModeSelectedIndex = 2;
                break;
            case PororocaHttpRequestBodyMode.Raw:
                RequestBodyModeSelectedIndex = 1;
                break;
            default:
                RequestBodyModeSelectedIndex = 0;
                break;
        }
        // RAW
        RequestRawContentType = req.Body?.ContentType;
        RequestRawContent = req.Body?.RawContent;
        // FILE
        RequestFileContentType = req.Body?.ContentType;
        RequestBodyFileSrcPath = req.Body?.FileSrcPath;
        SearchRequestBodyRawFileCmd = ReactiveCommand.CreateFromTask(SearchRequestBodyRawFilePathAsync);
        // URL ENCODED
        UrlEncodedParams = new();
        if (req.Body?.UrlEncodedValues is not null)
        {
            foreach (var v in req.Body.UrlEncodedValues)
            {
                UrlEncodedParams.Add(new(UrlEncodedParams, v));
            }
        }
        AddNewUrlEncodedParamCmd = ReactiveCommand.Create(AddNewUrlEncodedParam);
        // FORM DATA
        FormDataParams = new();
        if (req.Body?.FormDataValues is not null)
        {
            foreach (var v in req.Body.FormDataValues)
            {
                FormDataParams.Add(new(FormDataParams, v));
            }
        }
        AddNewFormDataTextParamCmd = ReactiveCommand.Create(AddNewFormDataTextParam);
        AddNewFormDataFileParamCmd = ReactiveCommand.CreateFromTask(AddNewFormDataFileParam);
        // GRAPHQL
        RequestBodyGraphQlQuery = req.Body?.GraphQlValues?.Query;
        RequestBodyGraphQlVariables = req.Body?.GraphQlValues?.Variables;
        #endregion

        #region REQUEST AUTH
        RequestAuthDataCtx = new(req.CustomAuth);
        #endregion

        #region SEND OR CANCEL REQUEST
        CancelRequestCmd = ReactiveCommand.Create(CancelRequest);
        SendRequestCmd = ReactiveCommand.CreateFromTask(SendRequestAsync);
        #endregion

        #region RESPONSE
        ResponseDataCtx = new();
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
        ResolvedRequestUrlToolTip = this.variableResolver.ReplaceTemplates(RequestUrl);

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

    #region REQUEST HEADERS

    private void AddNewHeader() =>
        RequestHeaders.Add(new(RequestHeaders, true, string.Empty, string.Empty));

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

    #region REQUEST BODY URL ENCODED

    private void AddNewUrlEncodedParam() =>
        UrlEncodedParams.Add(new(UrlEncodedParams, true, string.Empty, string.Empty));

    #endregion

    #region REQUEST BODY FORM DATA

    private void AddNewFormDataTextParam()
    {
        PororocaHttpRequestFormDataParam p = new(true, string.Empty);
        p.SetTextValue(string.Empty, MimeTypesDetector.DefaultMimeTypeForText);
        FormDataParams.Add(new(FormDataParams, p));
    }

    private async Task AddNewFormDataFileParam()
    {
        string? fileSrcPath = await SearchFileWithDialogAsync();
        if (fileSrcPath != null)
        {
            MimeTypesDetector.TryFindMimeTypeForFile(fileSrcPath, out string? mimeType);
            mimeType ??= MimeTypesDetector.DefaultMimeTypeForBinary;

            PororocaHttpRequestFormDataParam p = new(true, string.Empty);
            p.SetFileValue(fileSrcPath, mimeType);
            FormDataParams.Add(new(FormDataParams, p));
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
                body.SetFormDataContent(FormDataParams.Select(p => p.ToFormDataParam()));
                break;
            case PororocaHttpRequestBodyMode.UrlEncoded:
                body.SetUrlEncodedContent(UrlEncodedParams.Select(p => p.ToKeyValueParam()));
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
            headers: RequestHeaders.Count == 0 ? null : RequestHeaders.Select(h => h.ToKeyValueParam()),
            body: WrapRequestBodyFromInputs());

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
    }

    private void ShowInvalidRequestWarnings()
    {
        string? errorCode = this.invalidRequestMessageErrorCode;
        InvalidRequestMessage = errorCode switch
        {
            TranslateRequestErrors.ClientCertificateFileNotFound => Localizer.Instance.RequestValidation.ClientCertificateFileNotFound,
            TranslateRequestErrors.ClientCertificatePkcs12PasswordCannotBeBlank => Localizer.Instance.RequestValidation.ClientCertificatePkcs12PasswordCannotBeBlank,
            TranslateRequestErrors.ClientCertificatePrivateKeyFileNotFound => Localizer.Instance.RequestValidation.ClientCertificatePrivateKeyFileNotFound,
            TranslateRequestErrors.ContentTypeCannotBeBlankReqBodyRaw => Localizer.Instance.RequestValidation.ContentTypeCannotBeBlankReqBodyRawOrFile,
            TranslateRequestErrors.ContentTypeCannotBeBlankReqBodyFile => Localizer.Instance.RequestValidation.ContentTypeCannotBeBlankReqBodyRawOrFile,
            TranslateRequestErrors.Http2UnavailableInOSVersion => Localizer.Instance.RequestValidation.Http2Unavailable,
            TranslateRequestErrors.Http3UnavailableInOSVersion => Localizer.Instance.RequestValidation.Http3Unavailable,
            TranslateRequestErrors.InvalidContentTypeFormData => Localizer.Instance.RequestValidation.InvalidContentTypeFormData,
            TranslateRequestErrors.InvalidContentTypeRaw => Localizer.Instance.RequestValidation.InvalidContentTypeRawOrFile,
            TranslateRequestErrors.InvalidContentTypeFile => Localizer.Instance.RequestValidation.InvalidContentTypeRawOrFile,
            TranslateRequestErrors.InvalidUrl => Localizer.Instance.RequestValidation.InvalidUrl,
            TranslateRequestErrors.ReqBodyFileNotFound => Localizer.Instance.RequestValidation.ReqBodyFileNotFound,
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