using System.Reactive;
using Pororoca.Desktop.Controls;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.ExportImport;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.TranslateRequest;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class RequestAuthViewModel : ViewModelBase
{
    internal CollectionViewModel Collection { get; }
    internal PororocaVariableSyntaxHighlightingDefinitionSet PororocaVarSyntaxHighlightingDefinitionSet { get; }

    #region REQUEST AUTH

    private readonly Action clearInvalidWarningsCallback;

    public int AuthModeSelectedIndex
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
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

    public string? ClientCertificateAuthPkcs12CertificateFilePath
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasClientCertificateAuthPkcs12CertificateFilePathProblem)
                this.clearInvalidWarningsCallback();
        }
    }

    public string? ClientCertificateAuthPkcs12FilePassword
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
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

    public string? ClientCertificateAuthPemCertificateFilePath
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasClientCertificateAuthPemCertificateFilePathProblem)
                this.clearInvalidWarningsCallback();
        }
    }

    public string? ClientCertificateAuthPemPrivateKeyFilePath
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
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

    public bool WindowsAuthUseCurrentUser
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasWindowsAuthLoginProblem || HasWindowsAuthPasswordProblem || HasWindowsAuthDomainProblem)
                this.clearInvalidWarningsCallback();
        }
    }

    public string? WindowsAuthLogin
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasWindowsAuthLoginProblem)
                this.clearInvalidWarningsCallback();
        }
    }

    public string? WindowsAuthPassword
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasWindowsAuthPasswordProblem)
                this.clearInvalidWarningsCallback();
        }
    }

    public string? WindowsAuthDomain
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
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

    public RequestAuthViewModel(CollectionViewModel col, PororocaRequestAuth? customAuth, bool isInheritFromCollectionOptionEnabled, Action clearInvalidWarningsCallback)
    {
        Collection = col;
        PororocaVarSyntaxHighlightingDefinitionSet = Collection.PororocaVarSyntaxHighlightingDefinitionSet;
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

    #region VALIDATIONS

    public void HighlightValidationProblems(string? errorCode)
    {
        HasWindowsAuthLoginProblem = errorCode == TranslateRequestErrors.WindowsAuthLoginCannotBeBlank;
        HasWindowsAuthPasswordProblem = errorCode == TranslateRequestErrors.WindowsAuthPasswordCannotBeBlank;
        HasWindowsAuthDomainProblem = errorCode == TranslateRequestErrors.WindowsAuthDomainCannotBeBlank;
        HasClientCertificateAuthPkcs12CertificateFilePathProblem = errorCode == TranslateRequestErrors.ClientCertificatePkcs12CertificateFileNotFound;
        HasClientCertificateAuthPkcs12FilePasswordProblem = errorCode == TranslateRequestErrors.ClientCertificatePkcs12PasswordCannotBeBlank;
        HasClientCertificateAuthPemCertificateFilePathProblem = errorCode == TranslateRequestErrors.ClientCertificatePemCertificateFileNotFound;
        HasClientCertificateAuthPemPrivateKeyFilePathProblem = errorCode == TranslateRequestErrors.ClientCertificatePemPrivateKeyFileNotFound;
    }

    #endregion

    #endregion
}