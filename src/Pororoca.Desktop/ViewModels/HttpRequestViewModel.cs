using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaEdit.Document;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;
using Pororoca.Infrastructure.Features.Requester;
using ReactiveUI;
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
    private readonly Guid reqId;

    private int requestTabsSelectedIndexField;
    public int RequestTabsSelectedIndex // To preserve the state of the last shown request tab
    {
        get => this.requestTabsSelectedIndexField;
        set => this.RaiseAndSetIfChanged(ref this.requestTabsSelectedIndexField, value);
    }

    #region REQUEST HTTP METHOD
    public ObservableCollection<string> RequestMethodSelectionOptions { get; }
    private int requestMethodSelectedIndexField;
    public int RequestMethodSelectedIndex
    {
        get => this.requestMethodSelectedIndexField;
        set => this.RaiseAndSetIfChanged(ref this.requestMethodSelectedIndexField, value);
    }
    private HttpMethod RequestMethod =>
        AvailableHttpMethods[this.requestMethodSelectedIndexField];

    #endregion

    #region REQUEST URL

    private string requestUrlField;
    public string RequestUrl
    {
        get => this.requestUrlField;
        set => this.RaiseAndSetIfChanged(ref this.requestUrlField, value);
    }

    private string resolvedRequestUrlToolTipField;
    public string ResolvedRequestUrlToolTip
    {
        get => this.resolvedRequestUrlToolTipField;
        set => this.RaiseAndSetIfChanged(ref this.resolvedRequestUrlToolTipField, value);
    }

    #endregion

    #region REQUEST HTTP VERSION

    public ObservableCollection<string> RequestHttpVersionSelectionOptions { get; }
    private int requestHttpVersionSelectedIndexField;
    public int RequestHttpVersionSelectedIndex
    {
        get => this.requestHttpVersionSelectedIndexField;
        set => this.RaiseAndSetIfChanged(ref this.requestHttpVersionSelectedIndexField, value);
    }
    private decimal RequestHttpVersion =>
        AvailableHttpVersionsForHttp[this.requestHttpVersionSelectedIndexField];

    #endregion

    #region REQUEST VALIDATION MESSAGE

    private string? invalidRequestMessageErrorCode;

    private bool isInvalidRequestMessageVisibleField;
    public bool IsInvalidRequestMessageVisible
    {
        get => this.isInvalidRequestMessageVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isInvalidRequestMessageVisibleField, value);
    }
    private string? invalidRequestMessageField;
    public string? InvalidRequestMessage
    {
        get => this.invalidRequestMessageField;
        set => this.RaiseAndSetIfChanged(ref this.invalidRequestMessageField, value);
    }

    #endregion

    #region REQUEST HEADERS

    public ObservableCollection<KeyValueParamViewModel> RequestHeaders { get; }
    public KeyValueParamViewModel? SelectedRequestHeader { get; set; }
    public ReactiveCommand<Unit, Unit> AddNewRequestHeaderCmd { get; }
    public ReactiveCommand<Unit, Unit> RemoveSelectedRequestHeaderCmd { get; }

    #endregion

    #region REQUEST BODY

    private int requestBodyModeSelectedIndexField;
    public int RequestBodyModeSelectedIndex
    {
        get => this.requestBodyModeSelectedIndexField;
        set => this.RaiseAndSetIfChanged(ref this.requestBodyModeSelectedIndexField, value);
    }

    private bool isRequestBodyModeNoneSelectedField;
    public bool IsRequestBodyModeNoneSelected
    {
        get => this.isRequestBodyModeNoneSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isRequestBodyModeNoneSelectedField, value);
    }

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

    private bool isRequestBodyModeRawSelectedField;
    public bool IsRequestBodyModeRawSelected
    {
        get => this.isRequestBodyModeRawSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isRequestBodyModeRawSelectedField, value);
    }

    private string? requestRawContentTypeField;
    public string? RequestRawContentType
    {
        get => this.requestRawContentTypeField;
        set => this.RaiseAndSetIfChanged(ref this.requestRawContentTypeField, value);
    }

    private TextDocument? requestRawContentTextDocumentField;
    public TextDocument? RequestRawContentTextDocument
    {
        get => this.requestRawContentTextDocumentField;
        set => this.RaiseAndSetIfChanged(ref this.requestRawContentTextDocumentField, value);
    }

    public string? RequestRawContent
    {
        get => RequestRawContentTextDocument?.Text;
        set => RequestRawContentTextDocument = new(value ?? string.Empty);
    }

    #endregion

    #region REQUEST BODY FILE

    private bool isRequestBodyModeFileSelectedField;
    public bool IsRequestBodyModeFileSelected
    {
        get => this.isRequestBodyModeFileSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isRequestBodyModeFileSelectedField, value);
    }

    private string? requestFileContentTypeField;
    public string? RequestFileContentType
    {
        get => this.requestFileContentTypeField;
        set => this.RaiseAndSetIfChanged(ref this.requestFileContentTypeField, value);
    }

    private string? requestBodyFileSrcPathField;
    public string? RequestBodyFileSrcPath
    {
        get => this.requestBodyFileSrcPathField;
        set => this.RaiseAndSetIfChanged(ref this.requestBodyFileSrcPathField, value);
    }
    public ReactiveCommand<Unit, Unit> SearchRequestBodyRawFileCmd { get; }

    #endregion

    #region REQUEST BODY URL ENCODED

    private bool isRequestBodyModeUrlEncodedSelectedField;
    public bool IsRequestBodyModeUrlEncodedSelected
    {
        get => this.isRequestBodyModeUrlEncodedSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isRequestBodyModeUrlEncodedSelectedField, value);
    }

    public ObservableCollection<KeyValueParamViewModel> UrlEncodedParams { get; }
    public KeyValueParamViewModel? SelectedUrlEncodedParam { get; set; }
    public ReactiveCommand<Unit, Unit> AddNewUrlEncodedParamCmd { get; }
    public ReactiveCommand<Unit, Unit> RemoveSelectedUrlEncodedParamCmd { get; }

    #endregion

    #region REQUEST BODY FORM DATA

    private bool isRequestBodyModeFormDataSelectedField;
    public bool IsRequestBodyModeFormDataSelected
    {
        get => this.isRequestBodyModeFormDataSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isRequestBodyModeFormDataSelectedField, value);
    }

    public ObservableCollection<HttpRequestFormDataParamViewModel> FormDataParams { get; }
    public HttpRequestFormDataParamViewModel? SelectedFormDataParam { get; set; }
    public ReactiveCommand<Unit, Unit> AddNewFormDataTextParamCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewFormDataFileParamCmd { get; }
    public ReactiveCommand<Unit, Unit> RemoveSelectedFormDataParamCmd { get; }

    #endregion

    #region REQUEST BODY GRAPHQL

    private bool isRequestBodyModeGraphQlSelectedField;
    public bool IsRequestBodyModeGraphQlSelected
    {
        get => this.isRequestBodyModeGraphQlSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isRequestBodyModeGraphQlSelectedField, value);
    }

    private string? requestBodyGraphQlQueryField;
    public string? RequestBodyGraphQlQuery
    {
        get => this.requestBodyGraphQlQueryField;
        set => this.RaiseAndSetIfChanged(ref this.requestBodyGraphQlQueryField, value);
    }

    private string? requestBodyGraphQlVariablesField;
    public string? RequestBodyGraphQlVariables
    {
        get => this.requestBodyGraphQlVariablesField;
        set => this.RaiseAndSetIfChanged(ref this.requestBodyGraphQlVariablesField, value);
    }

    #endregion

    #endregion

    #region REQUEST AUTH

    private RequestAuthViewModel requestAuthDataCtxField;
    public RequestAuthViewModel RequestAuthDataCtx
    {
        get => this.requestAuthDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.requestAuthDataCtxField, value);
    }

    #endregion

    #endregion

    #region SEND OR CANCEL REQUEST

    private bool isRequestingField;
    public bool IsRequesting
    {
        get => this.isRequestingField;
        set => this.RaiseAndSetIfChanged(ref this.isRequestingField, value);
    }

    private CancellationTokenSource? sendRequestCancellationTokenSourceField;

    public ReactiveCommand<Unit, Unit> CancelRequestCmd { get; }
    public ReactiveCommand<Unit, Unit> SendRequestCmd { get; }

    private bool isSendRequestProgressBarVisibleField;
    public bool IsSendRequestProgressBarVisible
    {
        get => this.isSendRequestProgressBarVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isSendRequestProgressBarVisibleField, value);
    }

    #endregion

    #region RESPONSE

    private HttpResponseViewModel responseDataCtxField;
    public HttpResponseViewModel ResponseDataCtx
    {
        get => this.responseDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.responseDataCtxField, value);
    }

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
        this.reqId = req.Id;

        #endregion

        #region REQUEST HTTP METHOD, HTTP VERSION, URL AND HEADERS
        RequestMethodSelectionOptions = new(AvailableHttpMethods.Select(m => m.ToString()));
        int reqMethodSelectionIndex = RequestMethodSelectionOptions.IndexOf(req.HttpMethod);
        RequestMethodSelectedIndex = reqMethodSelectionIndex >= 0 ? reqMethodSelectionIndex : 0;

        this.resolvedRequestUrlToolTipField = this.requestUrlField = req.Url;

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
        this.requestAuthDataCtxField = new(req.CustomAuth);
        #endregion

        #region SEND OR CANCEL REQUEST
        CancelRequestCmd = ReactiveCommand.Create(CancelRequest);
        SendRequestCmd = ReactiveCommand.CreateFromTask(SendRequestAsync);
        #endregion

        #region RESPONSE
        this.responseDataCtxField = new();
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
        PororocaHttpRequest newReq = new(this.reqId, Name);
        UpdateRequestWithInputs(newReq);
        return newReq;
    }

    private void UpdateRequestWithInputs(PororocaHttpRequest request) =>
        request.Update(
            name: Name,
            httpVersion: RequestHttpVersion,
            httpMethod: RequestMethod.ToString(),
            url: RequestUrl,
            customAuth: this.requestAuthDataCtxField.ToCustomAuth(),
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

    private static async Task<string?> SearchFileWithDialogAsync()
    {
        OpenFileDialog dialog = new();
        string[]? result = await dialog.ShowAsync(MainWindow.Instance!);
        return result?.FirstOrDefault();
    }

    #endregion
}