using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;
using Pororoca.Infrastructure.Features.Requester;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using System.Reflection;

namespace Pororoca.Desktop.ViewModels
{
    public sealed class RequestViewModel : CollectionOrganizationItemViewModel
    {
        #region COLLECTION ORGANIZATION

        public ReactiveCommand<Unit, Unit> MoveUpCmd { get; }
        public ReactiveCommand<Unit, Unit> MoveDownCmd { get; }
        public ReactiveCommand<Unit, Unit> CopyRequestCmd { get; }
        public ReactiveCommand<Unit, Unit> RenameRequestCmd { get; }
        public ReactiveCommand<Unit, Unit> DeleteRequestCmd { get; }

        #endregion

        #region REQUEST

        private readonly IPororocaRequester _requester = PororocaRequester.Singleton;
        private readonly IPororocaVariableResolver _variableResolver;
        private readonly Guid _reqId;

        private int _requestTabsSelectedIndex;
        public int RequestTabsSelectedIndex // To preserve the state of the last shown request tab
        {
            get => _requestTabsSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _requestTabsSelectedIndex, value);
            }
        }

        #region REQUEST HTTP METHOD
        public ObservableCollection<string> RequestMethodSelectionOptions { get; }
        private int _requestMethodSelectedIndex;
        public int RequestMethodSelectedIndex
        {
            get => _requestMethodSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _requestMethodSelectedIndex, value);
            }
        }
        private HttpMethod RequestMethod =>
            AvailableHttpMethods[_requestMethodSelectedIndex];

        #endregion

        #region REQUEST URL

        private string _requestUrl;
        public string RequestUrl
        {
            get => _requestUrl;
            set
            {
                this.RaiseAndSetIfChanged(ref _requestUrl, value);
            }
        }

        private string _resolvedRequestUrlToolTip;
        public string ResolvedRequestUrlToolTip
        {
            get => _resolvedRequestUrlToolTip;
            set
            {
                this.RaiseAndSetIfChanged(ref _resolvedRequestUrlToolTip, value);
            }
        }

        #endregion

        #region REQUEST HTTP VERSION

        public ObservableCollection<string> RequestHttpVersionSelectionOptions { get; }
        private int _requestHttpVersionSelectedIndex;
        public int RequestHttpVersionSelectedIndex
        {
            get => _requestHttpVersionSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _requestHttpVersionSelectedIndex, value);
            }
        }
        private decimal RequestHttpVersion =>
            AvailableHttpVersions[_requestHttpVersionSelectedIndex];

        #endregion

        #region REQUEST VALIDATION MESSAGE

        private string? _invalidRequestMessageErrorCode;

        private bool _isInvalidRequestMessageVisible;
        public bool IsInvalidRequestMessageVisible
        {
            get => _isInvalidRequestMessageVisible;
            set
            {
                this.RaiseAndSetIfChanged(ref _isInvalidRequestMessageVisible, value);
            }
        }
        private string? _invalidRequestMessage;
        public string? InvalidRequestMessage
        {
            get => _invalidRequestMessage;
            set
            {
                this.RaiseAndSetIfChanged(ref _invalidRequestMessage, value);
            }
        }

        #endregion

        #region REQUEST HEADERS

        public ObservableCollection<KeyValueParamViewModel> RequestHeaders { get; }
        public KeyValueParamViewModel? SelectedRequestHeader { get; set; }
        public ReactiveCommand<Unit, Unit> AddNewRequestHeaderCmd { get; }
        public ReactiveCommand<Unit, Unit> RemoveSelectedRequestHeaderCmd { get; }

        #endregion

        #region REQUEST BODY

        private int _requestBodyModeSelectedIndex;
        public int RequestBodyModeSelectedIndex
        {
            get => _requestBodyModeSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _requestBodyModeSelectedIndex, value);
            }
        }

        private bool _isRequestBodyModeNoneSelected;
        public bool IsRequestBodyModeNoneSelected
        {
            get => _isRequestBodyModeNoneSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRequestBodyModeNoneSelected, value);
            }
        }

        private PororocaRequestBodyMode? RequestBodyMode
        {
            get
            {
                if (IsRequestBodyModeFormDataSelected) return PororocaRequestBodyMode.FormData;
                if (IsRequestBodyModeUrlEncodedSelected) return PororocaRequestBodyMode.UrlEncoded;
                if (IsRequestBodyModeFileSelected) return PororocaRequestBodyMode.File;
                if (IsRequestBodyModeRawSelected) return PororocaRequestBodyMode.Raw;
                else return null;
            }
        }

        public static ObservableCollection<string> AllMimeTypes { get; }
            = new(MimeTypesDetector.AllMimeTypes);

        #region REQUEST BODY RAW

        private bool _isRequestBodyModeRawSelected;
        public bool IsRequestBodyModeRawSelected
        {
            get => _isRequestBodyModeRawSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRequestBodyModeRawSelected, value);
            }
        }

        private string? _requestRawContentType;
        public string? RequestRawContentType
        {
            get => _requestRawContentType;
            set
            {
                this.RaiseAndSetIfChanged(ref _requestRawContentType, value);
            }
        }

        private string? _requestRawContent;
        public string? RequestRawContent
        {
            get => _requestRawContent;
            set
            {
                this.RaiseAndSetIfChanged(ref _requestRawContent, value);
            }
        }

        #endregion

        #region REQUEST BODY FILE

        private bool _isRequestBodyModeFileSelected;
        public bool IsRequestBodyModeFileSelected
        {
            get => _isRequestBodyModeFileSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRequestBodyModeFileSelected, value);
            }
        }

        private string? _requestFileContentType;
        public string? RequestFileContentType
        {
            get => _requestFileContentType;
            set
            {
                this.RaiseAndSetIfChanged(ref _requestFileContentType, value);
            }
        }

        private string? _requestBodyFileSrcPath;
        public string? RequestBodyFileSrcPath
        {
            get => _requestBodyFileSrcPath;
            set
            {
                this.RaiseAndSetIfChanged(ref _requestBodyFileSrcPath, value);
            }
        }
        public ReactiveCommand<Unit, Unit> SearchRequestBodyRawFileCmd { get; }

        #endregion

        #region REQUEST BODY URL ENCODED

        private bool _isRequestBodyModeUrlEncodedSelected;
        public bool IsRequestBodyModeUrlEncodedSelected
        {
            get => _isRequestBodyModeUrlEncodedSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRequestBodyModeUrlEncodedSelected, value);
            }
        }

        public ObservableCollection<KeyValueParamViewModel> UrlEncodedParams { get; }
        public KeyValueParamViewModel? SelectedUrlEncodedParam { get; set; }
        public ReactiveCommand<Unit, Unit> AddNewUrlEncodedParamCmd { get; }
        public ReactiveCommand<Unit, Unit> RemoveSelectedUrlEncodedParamCmd { get; }

        #endregion

        #region REQUEST BODY FORM DATA

        private bool _isRequestBodyModeFormDataSelected;
        public bool IsRequestBodyModeFormDataSelected
        {
            get => _isRequestBodyModeFormDataSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRequestBodyModeFormDataSelected, value);
            }
        }

        public ObservableCollection<RequestFormDataParamViewModel> FormDataParams { get; }
        public RequestFormDataParamViewModel? SelectedFormDataParam { get; set; }
        public ReactiveCommand<Unit, Unit> AddNewFormDataTextParamCmd { get; }
        public ReactiveCommand<Unit, Unit> AddNewFormDataFileParamCmd { get; }
        public ReactiveCommand<Unit, Unit> RemoveSelectedFormDataParamCmd { get; }

        #endregion

        #endregion

        #region REQUEST AUTH

        private int _requestAuthModeSelectedIndex;
        public int RequestAuthModeSelectedIndex
        {
            get => _requestAuthModeSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _requestAuthModeSelectedIndex, value);
            }
        }

        private bool _isRequestAuthModeNoneSelected;
        public bool IsRequestAuthModeNoneSelected
        {
            get => _isRequestAuthModeNoneSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRequestAuthModeNoneSelected, value);
            }
        }

        private bool _isRequestAuthModeBasicSelected;
        public bool IsRequestAuthModeBasicSelected
        {
            get => _isRequestAuthModeBasicSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRequestAuthModeBasicSelected, value);
            }
        }

        private bool _isRequestAuthModeBearerSelected;
        public bool IsRequestAuthModeBearerSelected
        {
            get => _isRequestAuthModeBearerSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRequestAuthModeBearerSelected, value);
            }
        }

        private PororocaRequestAuthMode? RequestAuthMode =>
            _requestAuthModeSelectedIndex switch
            { // TODO: Improve this, do not use fixed integers to resolve mode
                2 => PororocaRequestAuthMode.Bearer,
                1 => PororocaRequestAuthMode.Basic,
                _ => null
            };

        private string? _requestBasicAuthLogin;
        public string? RequestBasicAuthLogin
        {
            get => _requestBasicAuthLogin;
            set
            {
                this.RaiseAndSetIfChanged(ref _requestBasicAuthLogin, value);
            }
        }

        private string? _requestBasicAuthPassword;
        public string? RequestBasicAuthPassword
        {
            get => _requestBasicAuthPassword;
            set
            {
                this.RaiseAndSetIfChanged(ref _requestBasicAuthPassword, value);
            }
        }

        private string? _requestBearerAuthToken;
        public string? RequestBearerAuthToken
        {
            get => _requestBearerAuthToken;
            set
            {
                this.RaiseAndSetIfChanged(ref _requestBearerAuthToken, value);
            }
        }

        #endregion

        #endregion

        #region SEND OR CANCEL REQUEST

        private bool _isRequesting;
        public bool IsRequesting
        {
            get => _isRequesting;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRequesting, value);
            }
        }

        private CancellationTokenSource? _sendRequestCancellationTokenSource;

        public ReactiveCommand<Unit, Unit> CancelRequestCmd { get; }
        public ReactiveCommand<Unit, Unit> SendRequestCmd { get; }

        private bool _isSendRequestProgressBarVisible;
        public bool IsSendRequestProgressBarVisible
        {
            get => _isSendRequestProgressBarVisible;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSendRequestProgressBarVisible, value);
            }
        }

        #endregion

        #region RESPONSE

        private ResponseViewModel _responseDataCtx;
        public ResponseViewModel ResponseDataCtx
        {
            get => _responseDataCtx;
            set
            {
                this.RaiseAndSetIfChanged(ref _responseDataCtx, value);
            }
        }

        #endregion

        public RequestViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                IPororocaVariableResolver variableResolver,
                                PororocaRequest req) : base(parentVm, req.Name)
        {
            #region COLLECTION ORGANIZATION
            Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);

            MoveUpCmd = ReactiveCommand.Create(MoveThisUp);
            MoveDownCmd = ReactiveCommand.Create(MoveThisDown);
            CopyRequestCmd = ReactiveCommand.Create(CopyThis);
            RenameRequestCmd = ReactiveCommand.Create(RenameThis);
            DeleteRequestCmd = ReactiveCommand.Create(DeleteThis);
            #endregion

            #region REQUEST

            _variableResolver = variableResolver;
            _reqId = req.Id;

            #endregion

            #region REQUEST HTTP METHOD, HTTP VERSION, URL AND HEADERS
            RequestMethodSelectionOptions = new(AvailableHttpMethods.Select(m => m.ToString()));
            int reqMethodSelectionIndex = RequestMethodSelectionOptions.IndexOf(req.HttpMethod);
            RequestMethodSelectedIndex = reqMethodSelectionIndex >= 0 ? reqMethodSelectionIndex : 0;

            _resolvedRequestUrlToolTip = _requestUrl = req.Url;

            RequestHttpVersionSelectionOptions = new(AvailableHttpVersions.Select(FormatHttpVersionString));
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
                case PororocaRequestBodyMode.FormData:
                    RequestBodyModeSelectedIndex = 4;
                    IsRequestBodyModeFormDataSelected = true;
                    break;
                case PororocaRequestBodyMode.UrlEncoded:
                    RequestBodyModeSelectedIndex = 3;
                    IsRequestBodyModeUrlEncodedSelected = true;
                    break;
                case PororocaRequestBodyMode.File:
                    RequestBodyModeSelectedIndex = 2;
                    IsRequestBodyModeFileSelected = true;
                    break;
                case PororocaRequestBodyMode.Raw:
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
            FormDataParams = new(req.Body?.FormDataValues?.Select(p => new RequestFormDataParamViewModel(p)) ?? Array.Empty<RequestFormDataParamViewModel>());
            AddNewFormDataTextParamCmd = ReactiveCommand.Create(AddNewFormDataTextParam);
            AddNewFormDataFileParamCmd = ReactiveCommand.CreateFromTask(AddNewFormDataFileParam);
            RemoveSelectedFormDataParamCmd = ReactiveCommand.Create(RemoveSelectedFormDataParam);
            #endregion

            #region REQUEST AUTH
            // TODO: Improve this, do not use fixed values to resolve index
            switch (req.CustomAuth?.Mode)
            {
                case PororocaRequestAuthMode.Basic:
                    RequestAuthModeSelectedIndex = 1;
                    IsRequestAuthModeBasicSelected = true;
                    break;
                case PororocaRequestAuthMode.Bearer:
                    RequestAuthModeSelectedIndex = 2;
                    IsRequestAuthModeBearerSelected = true;
                    break;
                default:
                    RequestAuthModeSelectedIndex = 0;
                    IsRequestAuthModeNoneSelected = true;
                    break;
            }
            RequestBasicAuthLogin = req.CustomAuth?.BasicAuthLogin;
            RequestBasicAuthPassword = req.CustomAuth?.BasicAuthPassword;
            RequestBearerAuthToken = req.CustomAuth?.BearerToken;
            #endregion    

            #region SEND OR CANCEL REQUEST
            CancelRequestCmd = ReactiveCommand.Create(CancelRequest);
            SendRequestCmd = ReactiveCommand.CreateFromTask(SendRequestAsync);
            #endregion

            #region RESPONSE
            _responseDataCtx = new();
            #endregion
        }

        #region COLLECTION ORGANIZATION

        protected override void CopyThis() =>
            ClipboardAreaDataCtx.PushToCopy(ToRequest());

        private void OnLanguageChanged()
        {
            if (_invalidRequestMessageErrorCode != null && IsInvalidRequestMessageVisible)
            {
                ShowInvalidRequestMessage();
            }
        }

        #endregion

        #region REQUEST HTTP METHOD, HTTP VERSION AND URL

        public void UpdateResolvedRequestUrlToolTip() =>
            ResolvedRequestUrlToolTip = _variableResolver.ReplaceTemplates(RequestUrl);

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
            PororocaRequestFormDataParam p = new(true, string.Empty);
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

                PororocaRequestFormDataParam p = new(true, string.Empty);
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

        private PororocaRequestAuth? WrapCustomAuthFromInputs()
        {
            PororocaRequestAuth auth = new();
            switch (RequestAuthMode)
            {
                case PororocaRequestAuthMode.Bearer:
                    auth.SetBearerAuth(RequestBearerAuthToken ?? string.Empty);
                    break;
                case PororocaRequestAuthMode.Basic:
                    auth.SetBasicAuth(RequestBasicAuthLogin ?? string.Empty, RequestBasicAuthPassword ?? string.Empty);
                    break;
                default:
                    return null;
            }
            return auth;
        }

        private PororocaRequestBody? WrapRequestBodyFromInputs()
        {
            PororocaRequestBody body = new();
            switch (RequestBodyMode)
            {
                case PororocaRequestBodyMode.FormData:
                    body.SetFormDataContent(FormDataParams.Select(p => p.ToFormDataParam()));
                    break;
                case PororocaRequestBodyMode.UrlEncoded:
                    body.SetUrlEncodedContent(UrlEncodedParams.Select(p => p.ToKeyValueParam()));
                    break;
                case PororocaRequestBodyMode.File:
                    body.SetFileContent(RequestBodyFileSrcPath ?? string.Empty, RequestFileContentType ?? string.Empty);
                    break;
                case PororocaRequestBodyMode.Raw:
                    body.SetRawContent(RequestRawContent ?? string.Empty, RequestRawContentType ?? string.Empty);
                    break;
                default:
                    return null;
            }
            return body;
        }

        public PororocaRequest ToRequest()
        {
            PororocaRequest newReq = new(_reqId, Name);
            UpdateRequestWithInputs(newReq);
            return newReq;
        }

        private void UpdateRequestWithInputs(PororocaRequest request) =>
            request.Update(
                name: Name,
                httpVersion: RequestHttpVersion,
                httpMethod: RequestMethod.ToString(),
                url: RequestUrl,
                customAuth: WrapCustomAuthFromInputs(),
                headers: RequestHeaders.Count == 0 ? null : RequestHeaders.Select(h => h.ToKeyValueParam()),
                body: WrapRequestBodyFromInputs());

        #endregion

        #region SEND OR CANCEL REQUEST

        private void CancelRequest()
        {
            // CancellationToken will cause a failed PororocaResponse,
            // therefore is not necessary to here call method ShowNotSendingRequestUI()
            _sendRequestCancellationTokenSource!.Cancel();
        }

        private async Task SendRequestAsync()
        {
            ClearInvalidRequestMessage();
            PororocaRequest generatedReq = ToRequest();
            if (!_requester.IsValidRequest(_variableResolver, generatedReq, out string? errorCode))
            {
                _invalidRequestMessageErrorCode = errorCode;
                ShowInvalidRequestMessage();
            }
            else
            {
                ShowSendingRequestUI();
                PororocaResponse res = await SendRequestAsync(generatedReq);
                ResponseDataCtx.UpdateWithResponse(res);
                ShowNotSendingRequestUI();
            }
        }

        private void ClearInvalidRequestMessage()
        {
            _invalidRequestMessageErrorCode = null;
            IsInvalidRequestMessageVisible = false;
        }

        private void ShowInvalidRequestMessage()
        {
            InvalidRequestMessage = _invalidRequestMessageErrorCode switch
            {
                TranslateRequestErrors.InvalidUrl => Localizer.Instance["Request/ValidationInvalidUrl"],
                TranslateRequestErrors.Http3UnavailableInOSVersion => Localizer.Instance["Request/ValidationHttp3Unavailable"],
                TranslateRequestErrors.Http2UnavailableInOSVersion => Localizer.Instance["Request/ValidationHttp2Unavailable"],
                TranslateRequestErrors.ContentTypeCannotBeBlankReqBodyRawOrFile => Localizer.Instance["Request/ContentTypeCannotBeBlankReqBodyRawOrFile"],
                TranslateRequestErrors.InvalidContentTypeRawOrFile => Localizer.Instance["Request/InvalidContentTypeRawOrFile"],
                TranslateRequestErrors.InvalidContentTypeFormData => Localizer.Instance["Request/InvalidContentTypeFormData"],
                TranslateRequestErrors.ReqBodyFileNotFound => Localizer.Instance["Request/ReqBodyFileNotFound"],
                _ => Localizer.Instance["Request/ValidationInvalidUnknownCause"]
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

        private Task<PororocaResponse> SendRequestAsync(PororocaRequest generatedReq)
        {
            bool disableSslVerification = ((MainWindowViewModel)MainWindow.Instance!.DataContext!).IsSslVerificationDisabled;
            _sendRequestCancellationTokenSource = new();
            return _requester.RequestAsync(_variableResolver, generatedReq, disableSslVerification, _sendRequestCancellationTokenSource.Token);
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
}