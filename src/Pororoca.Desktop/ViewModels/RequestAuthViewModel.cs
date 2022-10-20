using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using Avalonia.Controls;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public sealed class RequestAuthViewModel : ViewModelBase
{
    #region REQUEST AUTH

    private int authModeSelectedIndexField;
    public int AuthModeSelectedIndex
    {
        get => this.authModeSelectedIndexField;
        set => this.RaiseAndSetIfChanged(ref this.authModeSelectedIndexField, value);
    }

    private bool isAuthModeNoneSelectedField;
    public bool IsAuthModeNoneSelected
    {
        get => this.isAuthModeNoneSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isAuthModeNoneSelectedField, value);
    }

    private bool isAuthModeBasicSelectedField;
    public bool IsAuthModeBasicSelected
    {
        get => this.isAuthModeBasicSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isAuthModeBasicSelectedField, value);
    }

    private bool isAuthModeBearerSelectedField;
    public bool IsAuthModeBearerSelected
    {
        get => this.isAuthModeBearerSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isAuthModeBearerSelectedField, value);
    }

    private bool isAuthModeClientCertificateSelectedField;
    public bool IsAuthModeClientCertificateSelected
    {
        get => this.isAuthModeClientCertificateSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isAuthModeClientCertificateSelectedField, value);
    }

    private PororocaRequestAuthMode? AuthMode =>
        this.authModeSelectedIndexField switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            3 => PororocaRequestAuthMode.ClientCertificate,
            2 => PororocaRequestAuthMode.Bearer,
            1 => PororocaRequestAuthMode.Basic,
            _ => null
        };

    #region REQUEST AUTH BASIC

    private string? basicAuthLoginField;
    public string? BasicAuthLogin
    {
        get => this.basicAuthLoginField;
        set => this.RaiseAndSetIfChanged(ref this.basicAuthLoginField, value);
    }

    private string? basicAuthPasswordField;
    public string? BasicAuthPassword
    {
        get => this.basicAuthPasswordField;
        set => this.RaiseAndSetIfChanged(ref this.basicAuthPasswordField, value);
    }

    #endregion

    #region REQUEST AUTH BEARER

    private string? bearerAuthTokenField;
    public string? BearerAuthToken
    {
        get => this.bearerAuthTokenField;
        set => this.RaiseAndSetIfChanged(ref this.bearerAuthTokenField, value);
    }

    #endregion

    #region REQUEST AUTH CLIENT CERTIFICATE

    private int clientCertificateTypeSelectedIndexField;
    public int ClientCertificateTypeSelectedIndex
    {
        get => this.clientCertificateTypeSelectedIndexField;
        set => this.RaiseAndSetIfChanged(ref this.clientCertificateTypeSelectedIndexField, value);
    }

    private bool isClientCertificateTypeNoneSelectedField;
    public bool IsClientCertificateTypeNoneSelected
    {
        get => this.isClientCertificateTypeNoneSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isClientCertificateTypeNoneSelectedField, value);
    }

    private bool isClientCertificateTypePkcs12SelectedField;
    public bool IsClientCertificateTypePkcs12Selected
    {
        get => this.isClientCertificateTypePkcs12SelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isClientCertificateTypePkcs12SelectedField, value);
    }

    private bool isClientCertificateTypePemSelectedField;
    public bool IsClientCertificateTypePemSelected
    {
        get => this.isClientCertificateTypePemSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isClientCertificateTypePemSelectedField, value);
    }

    private PororocaRequestAuthClientCertificateType? ClientCertificateType =>
        this.clientCertificateTypeSelectedIndexField switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            2 => PororocaRequestAuthClientCertificateType.Pem,
            1 => PororocaRequestAuthClientCertificateType.Pkcs12,
            _ => null
        };

    #region REQUEST AUTH CLIENT CERTIFICATE PKCS12

    private string? clientCertificateAuthPkcs12CertificateFilePathField;
    public string? ClientCertificateAuthPkcs12CertificateFilePath
    {
        get => this.clientCertificateAuthPkcs12CertificateFilePathField;
        set => this.RaiseAndSetIfChanged(ref this.clientCertificateAuthPkcs12CertificateFilePathField, value);
    }

    private string? clientCertificateAuthPkcs12FilePasswordField;
    public string? ClientCertificateAuthPkcs12FilePassword
    {
        get => this.clientCertificateAuthPkcs12FilePasswordField;
        set => this.RaiseAndSetIfChanged(ref this.clientCertificateAuthPkcs12FilePasswordField, value);
    }

    public ReactiveCommand<Unit, Unit> SearchClientCertificatePkcs12FileCmd { get; }

    #endregion

    #region REQUEST AUTH CLIENT CERTIFICATE PEM

    private string? clientCertificateAuthPemCertificateFilePathField;
    public string? ClientCertificateAuthPemCertificateFilePath
    {
        get => this.clientCertificateAuthPemCertificateFilePathField;
        set => this.RaiseAndSetIfChanged(ref this.clientCertificateAuthPemCertificateFilePathField, value);
    }

    private string? clientCertificateAuthPemPrivateKeyFilePathField;
    public string? ClientCertificateAuthPemPrivateKeyFilePath
    {
        get => this.clientCertificateAuthPemPrivateKeyFilePathField;
        set => this.RaiseAndSetIfChanged(ref this.clientCertificateAuthPemPrivateKeyFilePathField, value);
    }

    private string? clientCertificateAuthPemFilePasswordField;
    public string? ClientCertificateAuthPemFilePassword
    {
        get => this.clientCertificateAuthPemFilePasswordField;
        set => this.RaiseAndSetIfChanged(ref this.clientCertificateAuthPemFilePasswordField, value);
    }

    public ReactiveCommand<Unit, Unit> SearchClientCertificatePemCertFileCmd { get; }

    public ReactiveCommand<Unit, Unit> SearchClientCertificatePemPrivateKeyFileCmd { get; }

    #endregion

    #endregion

    #endregion

    #region OTHERS

    private readonly bool isOperatingSystemMacOsx;

    #endregion

    public RequestAuthViewModel(PororocaRequestAuth? customAuth, Func<bool>? isOperatingSystemMacOsx = null)
    {
        #region OTHERS
        this.isOperatingSystemMacOsx = (isOperatingSystemMacOsx ?? OperatingSystem.IsMacOS)();
        #endregion

        // TODO: Improve this, do not use fixed values to resolve index
        switch (customAuth?.Mode)
        {
            case PororocaRequestAuthMode.Basic:
                AuthModeSelectedIndex = 1;
                IsAuthModeBasicSelected = true;
                break;
            case PororocaRequestAuthMode.Bearer:
                AuthModeSelectedIndex = 2;
                IsAuthModeBearerSelected = true;
                break;
            case PororocaRequestAuthMode.ClientCertificate:
                AuthModeSelectedIndex = 3;
                IsAuthModeClientCertificateSelected = true;
                break;
            default:
                AuthModeSelectedIndex = 0;
                IsAuthModeNoneSelected = true;
                break;
        }
        BasicAuthLogin = customAuth?.BasicAuthLogin;
        BasicAuthPassword = customAuth?.BasicAuthPassword;
        BearerAuthToken = customAuth?.BearerToken;

        #region REQUEST AUTH CLIENT CERTIFICATE
        switch (customAuth?.ClientCertificate?.Type)
        {
            case PororocaRequestAuthClientCertificateType.Pkcs12:
                ClientCertificateAuthPkcs12CertificateFilePath = customAuth.ClientCertificate!.CertificateFilePath!;
                ClientCertificateAuthPkcs12FilePassword = customAuth.ClientCertificate!.FilePassword;
                ClientCertificateTypeSelectedIndex = 1;
                IsClientCertificateTypePkcs12Selected = true;
                break;
            case PororocaRequestAuthClientCertificateType.Pem:
                ClientCertificateAuthPemCertificateFilePath = customAuth.ClientCertificate!.CertificateFilePath!;
                ClientCertificateAuthPemPrivateKeyFilePath = customAuth.ClientCertificate!.PrivateKeyFilePath!;
                ClientCertificateAuthPemFilePassword = customAuth.ClientCertificate!.FilePassword;
                ClientCertificateTypeSelectedIndex = 2;
                IsClientCertificateTypePemSelected = true;
                break;
            default:
                ClientCertificateTypeSelectedIndex = 0;
                IsClientCertificateTypeNoneSelected = true;
                break;
        }
        SearchClientCertificatePkcs12FileCmd = ReactiveCommand.CreateFromTask(SearchClientCertificatePkcs12FileAsync);
        SearchClientCertificatePemCertFileCmd = ReactiveCommand.CreateFromTask(SearchClientCertificatePemCertFileAsync);
        SearchClientCertificatePemPrivateKeyFileCmd = ReactiveCommand.CreateFromTask(SearchClientCertificatePemPrivateKeyFileAsync);
    }

    #region REQUEST BODY AUTH CLIENT CERTIFICATE

    private async Task SearchClientCertificatePkcs12FileAsync()
    {
        List<FileDialogFilter> fileSelectionfilters = new();
        // Mac OSX file dialogs have problems with file filters... TODO: find if there is a way to solve this
        if (!this.isOperatingSystemMacOsx)
        {
            fileSelectionfilters.Add(
                new()
                {
                    Name = Localizer.Instance["RequestAuth/Pkcs12ImportCertificateFileTypesDescription"],
                    Extensions = new List<string> { "p12", "pfx" }
                }
            );
        }

        OpenFileDialog dialog = new()
        {
            Title = Localizer.Instance["RequestAuth/Pkcs12ImportCertificateFileDialogTitle"],
            AllowMultiple = false,
            Filters = fileSelectionfilters
        };
        string[]? result = await dialog.ShowAsync(MainWindow.Instance!);
        if (result != null)
        {
            ClientCertificateAuthPkcs12CertificateFilePath = result.FirstOrDefault();
        }
    }

    private async Task SearchClientCertificatePemCertFileAsync()
    {
        List<FileDialogFilter> fileSelectionfilters = new();
        // Mac OSX file dialogs have problems with file filters... TODO: find if there is a way to solve this
        if (!this.isOperatingSystemMacOsx)
        {
            fileSelectionfilters.Add(
                new()
                {
                    Name = Localizer.Instance["RequestAuth/PemImportCertificateFileTypesDescription"],
                    Extensions = new List<string> { "cer", "crt", "pem" }
                }
            );
        }

        OpenFileDialog dialog = new()
        {
            Title = Localizer.Instance["RequestAuth/PemImportCertificateFileDialogTitle"],
            AllowMultiple = false,
            Filters = fileSelectionfilters
        };
        string[]? result = await dialog.ShowAsync(MainWindow.Instance!);
        if (result != null)
        {
            ClientCertificateAuthPemCertificateFilePath = result.FirstOrDefault();
        }
    }

    private async Task SearchClientCertificatePemPrivateKeyFileAsync()
    {
        List<FileDialogFilter> fileSelectionfilters = new();
        // Mac OSX file dialogs have problems with file filters... TODO: find if there is a way to solve this
        if (!this.isOperatingSystemMacOsx)
        {
            fileSelectionfilters.Add(
                new()
                {
                    Name = Localizer.Instance["RequestAuth/PemImportPrivateKeyFileTypesDescription"],
                    Extensions = new List<string> { "cer", "crt", "pem", "key" }
                }
            );
        }

        OpenFileDialog dialog = new()
        {
            Title = Localizer.Instance["RequestAuth/PemImportPrivateKeyFileDialogTitle"],
            AllowMultiple = false,
            Filters = fileSelectionfilters
        };
        string[]? result = await dialog.ShowAsync(MainWindow.Instance!);
        if (result != null)
        {
            ClientCertificateAuthPemPrivateKeyFilePath = result.FirstOrDefault();
        }
    }

    #endregion

    #region CONVERT VIEW INPUTS TO REQUEST ENTITY

    public PororocaRequestAuth? ToCustomAuth()
    {
        PororocaRequestAuth auth = new();
        switch (AuthMode)
        {
            case PororocaRequestAuthMode.ClientCertificate:
                var type = ClientCertificateType;
                if (type == PororocaRequestAuthClientCertificateType.Pem)
                {
                    auth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, ClientCertificateAuthPemCertificateFilePath!, ClientCertificateAuthPemPrivateKeyFilePath, ClientCertificateAuthPemFilePassword);
                }
                else if (type == PororocaRequestAuthClientCertificateType.Pkcs12)
                {
                    auth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, ClientCertificateAuthPkcs12CertificateFilePath!, null, ClientCertificateAuthPkcs12FilePassword);
                }
                else
                {
                    return null;
                }
                break;
            case PororocaRequestAuthMode.Bearer:
                auth.SetBearerAuth(BearerAuthToken ?? string.Empty);
                break;
            case PororocaRequestAuthMode.Basic:
                auth.SetBasicAuth(BasicAuthLogin ?? string.Empty, BasicAuthPassword ?? string.Empty);
                break;
            default:
                return null;
        }
        return auth;
    }

    #endregion

    #endregion
}