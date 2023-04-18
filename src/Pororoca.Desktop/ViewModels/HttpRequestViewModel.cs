using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;
using AvaloniaEdit.Document;
using Pororoca.Desktop.ExportImport;
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
    #region COLLECTION ORGANIZATION

    public ReactiveCommand<Unit, Unit> MoveUpCmd { get; }
    public ReactiveCommand<Unit, Unit> MoveDownCmd { get; }
    public ReactiveCommand<Unit, Unit> CopyRequestCmd { get; }
    public ReactiveCommand<Unit, Unit> RenameRequestCmd { get; }
    public ReactiveCommand<Unit, Unit> DeleteRequestCmd { get; }

    #endregion

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

    [Reactive]
    public string RequestUrl { get; set; }

    [Reactive]
    public string ResolvedRequestUrlToolTip { get; set; }

    #endregion

    #region REQUEST HTTP VERSION

    public ObservableCollection<string> RequestHttpVersionSelectionOptions { get; }

    [Reactive]
    public int RequestHttpVersionSelectedIndex { get; set; }

    private decimal RequestHttpVersion =>
        AvailableHttpVersionsForHttp[RequestHttpVersionSelectedIndex];

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
    public KeyValueParamViewModel? SelectedRequestHeader { get; set; }
    public ReactiveCommand<Unit, Unit> AddNewRequestHeaderCmd { get; }
    public ReactiveCommand<Unit, Unit> RemoveSelectedRequestHeaderCmd { get; }

    #endregion

    #region REQUEST BODY

    [Reactive]
    public int RequestBodyModeSelectedIndex { get; set; }

    [Reactive]
    public bool IsRequestBodyModeNoneSelected { get; set; }

    private PororocaHttpRequestBodyMode? RequestBodyMode
    {
        get
        {
            if (IsRequestBodyModeFormDataSelected)
                return PororocaHttpRequestBodyMode.FormData;
            if (IsRequestBodyModeUrlEncodedSelected)
                return PororocaHttpRequestBodyMode.UrlEncoded;
            if (IsRequestBodyModeFileSelected)
                return PororocaHttpRequestBodyMode.File;
            if (IsRequestBodyModeRawSelected)
                return PororocaHttpRequestBodyMode.Raw;
            if (IsRequestBodyModeGraphQlSelected)
                return PororocaHttpRequestBodyMode.GraphQl;
            else
                return null;
        }
    }

    public static ObservableCollection<string> AllMimeTypes { get; } = new(MimeTypesDetector.AllMimeTypes);

    #region REQUEST BODY RAW

    [Reactive]
    public bool IsRequestBodyModeRawSelected { get; set; }

    [Reactive]
    public string? RequestRawContentType { get; set; }

    [Reactive]
    public TextDocument? RequestRawContentTextDocument { get; set; }

    public string? RequestRawContent
    {
        get => RequestRawContentTextDocument?.Text;
        set => RequestRawContentTextDocument = new(value ?? string.Empty);
    }

    #endregion

    #region REQUEST BODY FILE

    [Reactive]
    public bool IsRequestBodyModeFileSelected { get; set; }

    [Reactive]
    public string? RequestFileContentType { get; set; }

    [Reactive]
    public string? RequestBodyFileSrcPath { get; set; }

    public ReactiveCommand<Unit, Unit> SearchRequestBodyRawFileCmd { get; }

    #endregion

    #region REQUEST BODY URL ENCODED

    [Reactive]
    public bool IsRequestBodyModeUrlEncodedSelected { get; set; }

    public ObservableCollection<KeyValueParamViewModel> UrlEncodedParams { get; }
    public KeyValueParamViewModel? SelectedUrlEncodedParam { get; set; }
    public ReactiveCommand<Unit, Unit> AddNewUrlEncodedParamCmd { get; }
    public ReactiveCommand<Unit, Unit> RemoveSelectedUrlEncodedParamCmd { get; }

    #endregion

    #region REQUEST BODY FORM DATA

    [Reactive]
    public bool IsRequestBodyModeFormDataSelected { get; set; }

    public ObservableCollection<HttpRequestFormDataParamViewModel> FormDataParams { get; }
    public HttpRequestFormDataParamViewModel? SelectedFormDataParam { get; set; }
    public ReactiveCommand<Unit, Unit> AddNewFormDataTextParamCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewFormDataFileParamCmd { get; }
    public ReactiveCommand<Unit, Unit> RemoveSelectedFormDataParamCmd { get; }

    #endregion

    #region REQUEST BODY GRAPHQL

    [Reactive]
    public bool IsRequestBodyModeGraphQlSelected { get; set; }

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
        MoveUpCmd = ReactiveCommand.Create(MoveThisUp);
        MoveDownCmd = ReactiveCommand.Create(MoveThisDown);
        CopyRequestCmd = ReactiveCommand.Create(Copy);
        RenameRequestCmd = ReactiveCommand.Create(RenameThis);
        DeleteRequestCmd = ReactiveCommand.Create(Delete);
        #endregion

        #region REQUEST

        this.variableResolver = variableResolver;

        #endregion

        #region REQUEST HTTP METHOD, HTTP VERSION, URL AND HEADERS
        RequestMethodSelectionOptions = new(AvailableHttpMethods.Select(m => m.ToString()));
        int reqMethodSelectionIndex = RequestMethodSelectionOptions.IndexOf(req.HttpMethod);
        RequestMethodSelectedIndex = reqMethodSelectionIndex >= 0 ? reqMethodSelectionIndex : 0;

        ResolvedRequestUrlToolTip = RequestUrl = req.Url;

        RequestHttpVersionSelectionOptions = new(AvailableHttpVersionsForHttp.Select(FormatHttpVersionString));
        int reqHttpVersionSelectionIndex = RequestHttpVersionSelectionOptions.IndexOf(FormatHttpVersionString(req.HttpVersion));
        RequestHttpVersionSelectedIndex = reqHttpVersionSelectionIndex >= 0 ? reqHttpVersionSelectionIndex : 0;

        RequestHeaders = new(req.Headers?.Select(h => new KeyValueParamViewModel(h)) ?? Array.Empty<KeyValueParamViewModel>());
        AddNewRequestHeaderCmd = ReactiveCommand.Create(AddNewHeader);
        RemoveSelectedRequestHeaderCmd = ReactiveCommand.Create(RemoveSelectedHeader);
        #endregion

        #region REQUEST BODY
        // TODO: Improve this, do not use fixed values to resolve index
        switch (req.Body?.Mode)
        {
            case PororocaHttpRequestBodyMode.GraphQl:
                RequestBodyModeSelectedIndex = 5;
                IsRequestBodyModeGraphQlSelected = true;
                break;
            case PororocaHttpRequestBodyMode.FormData:
                RequestBodyModeSelectedIndex = 4;
                IsRequestBodyModeFormDataSelected = true;
                break;
            case PororocaHttpRequestBodyMode.UrlEncoded:
                RequestBodyModeSelectedIndex = 3;
                IsRequestBodyModeUrlEncodedSelected = true;
                break;
            case PororocaHttpRequestBodyMode.File:
                RequestBodyModeSelectedIndex = 2;
                IsRequestBodyModeFileSelected = true;
                break;
            case PororocaHttpRequestBodyMode.Raw:
                RequestBodyModeSelectedIndex = 1;
                IsRequestBodyModeRawSelected = true;
                break;
            default:
                RequestBodyModeSelectedIndex = 0;
                IsRequestBodyModeNoneSelected = true;
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
        UrlEncodedParams = new(req.Body?.UrlEncodedValues?.Select(p => new KeyValueParamViewModel(p)) ?? Array.Empty<KeyValueParamViewModel>());
        AddNewUrlEncodedParamCmd = ReactiveCommand.Create(AddNewUrlEncodedParam);
        RemoveSelectedUrlEncodedParamCmd = ReactiveCommand.Create(RemoveSelectedUrlEncodedParam);
        // FORM DATA
        FormDataParams = new(req.Body?.FormDataValues?.Select(p => new HttpRequestFormDataParamViewModel(p)) ?? Array.Empty<HttpRequestFormDataParamViewModel>());
        AddNewFormDataTextParamCmd = ReactiveCommand.Create(AddNewFormDataTextParam);
        AddNewFormDataFileParamCmd = ReactiveCommand.CreateFromTask(AddNewFormDataFileParam);
        RemoveSelectedFormDataParamCmd = ReactiveCommand.Create(RemoveSelectedFormDataParam);
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
        CollectionsGroupDataCtx.PushToCopy(ToHttpRequest());

    private void OnLanguageChanged()
    {
        if (this.invalidRequestMessageErrorCode != null && IsInvalidRequestMessageVisible)
        {
            ShowInvalidRequestMessage();
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
        RequestHeaders.Add(new(true, string.Empty, string.Empty));

    private void RemoveSelectedHeader()
    {
        if (SelectedRequestHeader != null)
        {
            RequestHeaders.Remove(SelectedRequestHeader);
            SelectedRequestHeader = null;
        }
        else if (RequestHeaders.Count == 1)
        {
            RequestHeaders.Clear();
        }
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

    #region REQUEST BODY URL ENCODED

    private void AddNewUrlEncodedParam() =>
        UrlEncodedParams.Add(new(true, string.Empty, string.Empty));

    private void RemoveSelectedUrlEncodedParam()
    {
        if (SelectedUrlEncodedParam != null)
        {
            UrlEncodedParams.Remove(SelectedUrlEncodedParam);
            SelectedUrlEncodedParam = null;
        }
        else if (UrlEncodedParams.Count == 1)
        {
            UrlEncodedParams.Clear();
        }
    }

    #endregion

    #region REQUEST BODY FORM DATA

    private void AddNewFormDataTextParam()
    {
        PororocaHttpRequestFormDataParam p = new(true, string.Empty);
        p.SetTextValue(string.Empty, MimeTypesDetector.DefaultMimeTypeForText);
        FormDataParams.Add(new(p));
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
            FormDataParams.Add(new(p));
        }
    }

    private void RemoveSelectedFormDataParam()
    {
        if (SelectedFormDataParam != null)
        {
            FormDataParams.Remove(SelectedFormDataParam);
            SelectedFormDataParam = null;
        }
        else if (FormDataParams.Count == 1)
        {
            FormDataParams.Clear();
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

    private void CancelRequest() =>
        // CancellationToken will cause a failed PororocaResponse,
        // therefore is not necessary to here call method ShowNotSendingRequestUI()
        this.sendRequestCancellationTokenSourceField!.Cancel();

    private async Task SendRequestAsync()
    {
        ClearInvalidRequestMessage();
        var generatedReq = ToHttpRequest();
        if (!this.requester.IsValidRequest(this.variableResolver, generatedReq, out string? errorCode))
        {
            this.invalidRequestMessageErrorCode = errorCode;
            ShowInvalidRequestMessage();
        }
        else
        {
            ShowSendingRequestUI();
            var res = await SendRequestAsync(generatedReq);
            ResponseDataCtx.UpdateWithResponse(res);
            ShowNotSendingRequestUI();
        }
    }

    private void ClearInvalidRequestMessage()
    {
        this.invalidRequestMessageErrorCode = null;
        IsInvalidRequestMessageVisible = false;
    }

    private void ShowInvalidRequestMessage()
    {
        InvalidRequestMessage = this.invalidRequestMessageErrorCode switch
        {
            TranslateRequestErrors.ClientCertificateFileNotFound => Localizer.Instance["RequestValidation/ClientCertificateFileNotFound"],
            TranslateRequestErrors.ClientCertificatePkcs12PasswordCannotBeBlank => Localizer.Instance["RequestValidation/ClientCertificatePkcs12PasswordCannotBeBlank"],
            TranslateRequestErrors.ClientCertificatePrivateKeyFileNotFound => Localizer.Instance["RequestValidation/ClientCertificatePrivateKeyFileNotFound"],
            TranslateRequestErrors.ContentTypeCannotBeBlankReqBodyRawOrFile => Localizer.Instance["RequestValidation/ContentTypeCannotBeBlankReqBodyRawOrFile"],
            TranslateRequestErrors.Http2UnavailableInOSVersion => Localizer.Instance["RequestValidation/Http2Unavailable"],
            TranslateRequestErrors.Http3UnavailableInOSVersion => Localizer.Instance["RequestValidation/Http3Unavailable"],
            TranslateRequestErrors.InvalidContentTypeFormData => Localizer.Instance["RequestValidation/InvalidContentTypeFormData"],
            TranslateRequestErrors.InvalidContentTypeRawOrFile => Localizer.Instance["RequestValidation/InvalidContentTypeRawOrFile"],
            TranslateRequestErrors.InvalidUrl => Localizer.Instance["RequestValidation/InvalidUrl"],
            TranslateRequestErrors.ReqBodyFileNotFound => Localizer.Instance["RequestValidation/ReqBodyFileNotFound"],
            _ => Localizer.Instance["RequestValidation/InvalidUnknownCause"]
        };
        IsInvalidRequestMessageVisible = true;
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