using System.Reactive;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.ExportImport;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class RequestAuthViewModel : ViewModelBase
{
    #region REQUEST AUTH

    private readonly Action clearInvalidWarningsCallback;

    private int authModeSelectedIndexField;
    public int AuthModeSelectedIndex
    {
        get => this.authModeSelectedIndexField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.authModeSelectedIndexField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasValidationProblem)
                this.clearInvalidWarningsCallback();
        }
    }

    internal PororocaRequestAuthMode? AuthMode =>
        AuthModeMapping.MapIndexToEnum(AuthModeSelectedIndex);

    public bool IsInheritFromCollectionOptionEnabled { get; }

    #region REQUEST AUTH BASIC

    [Reactive]
    public string? BasicAuthLogin { get; set; }

    [Reactive]
    public string? BasicAuthPassword { get; set; }

    #endregion

    #region REQUEST AUTH BEARER

    [Reactive]
    public string? BearerAuthToken { get; set; }

    #endregion

    #region REQUEST AUTH CLIENT CERTIFICATE

    [Reactive]
    public int ClientCertificateTypeSelectedIndex { get; set; }

    private PororocaRequestAuthClientCertificateType? ClientCertificateType =>
        ClientCertificateTypeMapping.MapIndexToEnum(ClientCertificateTypeSelectedIndex);

    #region REQUEST AUTH CLIENT CERTIFICATE PKCS12

    private string? clientCertificateAuthPkcs12CertificateFilePathField;
    public string? ClientCertificateAuthPkcs12CertificateFilePath
    {
        get => this.clientCertificateAuthPkcs12CertificateFilePathField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.clientCertificateAuthPkcs12CertificateFilePathField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasClientCertificateAuthPkcs12CertificateFilePathProblem)
                this.clearInvalidWarningsCallback();
        }
    }

    private string? clientCertificateAuthPkcs12FilePasswordField;
    public string? ClientCertificateAuthPkcs12FilePassword
    {
        get => this.clientCertificateAuthPkcs12FilePasswordField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.clientCertificateAuthPkcs12FilePasswordField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasClientCertificateAuthPkcs12FilePasswordProblem)
                this.clearInvalidWarningsCallback();
        }
    }

    [Reactive]
    public bool HasClientCertificateAuthPkcs12CertificateFilePathProblem { get; set; }

    [Reactive]
    public bool HasClientCertificateAuthPkcs12FilePasswordProblem { get; set; }

    public ReactiveCommand<Unit, Unit> SearchClientCertificatePkcs12FileCmd { get; }

    #endregion

    #region REQUEST AUTH CLIENT CERTIFICATE PEM

    private string? clientCertificateAuthPemCertificateFilePathField;
    public string? ClientCertificateAuthPemCertificateFilePath
    {
        get => this.clientCertificateAuthPemCertificateFilePathField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.clientCertificateAuthPemCertificateFilePathField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasClientCertificateAuthPemCertificateFilePathProblem)
                this.clearInvalidWarningsCallback();
        }
    }

    private string? clientCertificateAuthPemPrivateKeyFilePathField;
    public string? ClientCertificateAuthPemPrivateKeyFilePath
    {
        get => this.clientCertificateAuthPemPrivateKeyFilePathField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.clientCertificateAuthPemPrivateKeyFilePathField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasClientCertificateAuthPemPrivateKeyFilePathProblem)
                this.clearInvalidWarningsCallback();
        }
    }

    [Reactive]
    public bool HasClientCertificateAuthPemCertificateFilePathProblem { get; set; }

    [Reactive]
    public bool HasClientCertificateAuthPemPrivateKeyFilePathProblem { get; set; }

    [Reactive]
    public string? ClientCertificateAuthPemFilePassword { get; set; }

    public ReactiveCommand<Unit, Unit> SearchClientCertificatePemCertFileCmd { get; }

    public ReactiveCommand<Unit, Unit> SearchClientCertificatePemPrivateKeyFileCmd { get; }

    #endregion

    #endregion

    #region REQUEST AUTH WINDOWS

    private bool windowsAuthUseCurrentUserField;
    public bool WindowsAuthUseCurrentUser
    {
        get => this.windowsAuthUseCurrentUserField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.windowsAuthUseCurrentUserField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasWindowsAuthLoginProblem || HasWindowsAuthPasswordProblem || HasWindowsAuthDomainProblem)
                this.clearInvalidWarningsCallback();
        }
    }

    private string? windowsAuthLoginField;
    public string? WindowsAuthLogin
    {
        get => this.windowsAuthLoginField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.windowsAuthLoginField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasWindowsAuthLoginProblem)
                this.clearInvalidWarningsCallback();
        }
    }

    private string? windowsAuthPasswordField;
    public string? WindowsAuthPassword
    {
        get => this.windowsAuthPasswordField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.windowsAuthPasswordField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasWindowsAuthPasswordProblem)
                this.clearInvalidWarningsCallback();
        }
    }

    private string? windowsAuthDomainField;
    public string? WindowsAuthDomain
    {
        get => this.windowsAuthDomainField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.windowsAuthDomainField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasWindowsAuthDomainProblem)
                this.clearInvalidWarningsCallback();
        }
    }

    [Reactive]
    public bool HasWindowsAuthLoginProblem { get; set; }

    [Reactive]
    public bool HasWindowsAuthPasswordProblem { get; set; }

    [Reactive]
    public bool HasWindowsAuthDomainProblem { get; set; }

    #endregion

    public bool HasValidationProblem =>
        HasWindowsAuthLoginProblem
     || HasWindowsAuthPasswordProblem
     || HasWindowsAuthDomainProblem
     || HasClientCertificateAuthPkcs12CertificateFilePathProblem
     || HasClientCertificateAuthPkcs12FilePasswordProblem
     || HasClientCertificateAuthPemCertificateFilePathProblem
     || HasClientCertificateAuthPemPrivateKeyFilePathProblem;

    #endregion

    public RequestAuthViewModel(PororocaRequestAuth? customAuth, bool isInheritFromCollectionOptionEnabled, Action clearInvalidWarningsCallback)
    {
        this.clearInvalidWarningsCallback = clearInvalidWarningsCallback;
        AuthModeSelectedIndex = AuthModeMapping.MapEnumToIndex(customAuth?.Mode);
        IsInheritFromCollectionOptionEnabled = isInheritFromCollectionOptionEnabled;
        BasicAuthLogin = customAuth?.BasicAuthLogin;
        BasicAuthPassword = customAuth?.BasicAuthPassword;
        BearerAuthToken = customAuth?.BearerToken;
        WindowsAuthUseCurrentUser = customAuth?.Windows?.UseCurrentUser ?? false;
        WindowsAuthLogin = customAuth?.Windows?.Login;
        WindowsAuthPassword = customAuth?.Windows?.Password;
        WindowsAuthDomain = customAuth?.Windows?.Domain;

        #region REQUEST AUTH CLIENT CERTIFICATE
        ClientCertificateTypeSelectedIndex = ClientCertificateTypeMapping.MapEnumToIndex(customAuth?.ClientCertificate?.Type);
        switch (customAuth?.ClientCertificate?.Type)
        {
            case PororocaRequestAuthClientCertificateType.Pkcs12:
                ClientCertificateAuthPkcs12CertificateFilePath = customAuth.ClientCertificate!.CertificateFilePath!;
                ClientCertificateAuthPkcs12FilePassword = customAuth.ClientCertificate!.FilePassword;
                break;
            case PororocaRequestAuthClientCertificateType.Pem:
                ClientCertificateAuthPemCertificateFilePath = customAuth.ClientCertificate!.CertificateFilePath!;
                ClientCertificateAuthPemPrivateKeyFilePath = customAuth.ClientCertificate!.PrivateKeyFilePath!;
                ClientCertificateAuthPemFilePassword = customAuth.ClientCertificate!.FilePassword;
                break;
            default:
                break;
        }
        SearchClientCertificatePkcs12FileCmd = ReactiveCommand.CreateFromTask(SearchClientCertificatePkcs12FileAsync);
        SearchClientCertificatePemCertFileCmd = ReactiveCommand.CreateFromTask(SearchClientCertificatePemCertFileAsync);
        SearchClientCertificatePemPrivateKeyFileCmd = ReactiveCommand.CreateFromTask(SearchClientCertificatePemPrivateKeyFileAsync);
    }

    public void ClearRequestAuthValidationWarnings() =>
        HasWindowsAuthLoginProblem =
        HasWindowsAuthPasswordProblem =
        HasWindowsAuthDomainProblem =
        HasClientCertificateAuthPkcs12CertificateFilePathProblem =
        HasClientCertificateAuthPkcs12FilePasswordProblem =
        HasClientCertificateAuthPemCertificateFilePathProblem =
        HasClientCertificateAuthPemPrivateKeyFilePathProblem = false;

    #region REQUEST BODY AUTH CLIENT CERTIFICATE

    private async Task SearchClientCertificatePkcs12FileAsync()
    {
        string? result = await FileExporterImporter.SearchClientCertificatePkcs12FileAsync();
        if (result != null)
        {
            ClientCertificateAuthPkcs12CertificateFilePath = result;
        }
    }

    private async Task SearchClientCertificatePemCertFileAsync()
    {
        string? result = await FileExporterImporter.SearchClientCertificatePemCertFileAsync();
        if (result != null)
        {
            ClientCertificateAuthPemCertificateFilePath = result;
        }
    }

    private async Task SearchClientCertificatePemPrivateKeyFileAsync()
    {
        string? result = await FileExporterImporter.SearchClientCertificatePemPrivateKeyFileAsync();
        if (result != null)
        {
            ClientCertificateAuthPemPrivateKeyFilePath = result;
        }
    }

    #endregion

    #region CONVERT VIEW INPUTS TO REQUEST ENTITY

    public PororocaRequestAuth? ToCustomAuth()
    {
        switch (AuthMode)
        {
            case PororocaRequestAuthMode.ClientCertificate:
                var type = ClientCertificateType;
                if (type == PororocaRequestAuthClientCertificateType.Pem)
                {
                    return PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, ClientCertificateAuthPemCertificateFilePath!, ClientCertificateAuthPemPrivateKeyFilePath, ClientCertificateAuthPemFilePassword);
                }
                else if (type == PororocaRequestAuthClientCertificateType.Pkcs12)
                {
                    return PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, ClientCertificateAuthPkcs12CertificateFilePath!, null, ClientCertificateAuthPkcs12FilePassword);
                }
                else
                {
                    return null;
                }
            case PororocaRequestAuthMode.Windows:
                return PororocaRequestAuth.MakeWindowsAuth(WindowsAuthUseCurrentUser, WindowsAuthLogin, WindowsAuthPassword, WindowsAuthDomain);
            case PororocaRequestAuthMode.Bearer:
                return PororocaRequestAuth.MakeBearerAuth(BearerAuthToken ?? string.Empty);
            case PororocaRequestAuthMode.Basic:
                return PororocaRequestAuth.MakeBasicAuth(BasicAuthLogin ?? string.Empty, BasicAuthPassword ?? string.Empty);
            case PororocaRequestAuthMode.InheritFromCollection:
                return PororocaRequestAuth.InheritedFromCollection;
            default:
                return null;
        }
    }

    #endregion

    #endregion
}