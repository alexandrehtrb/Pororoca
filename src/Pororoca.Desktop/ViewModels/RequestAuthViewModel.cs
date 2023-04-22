using System.Reactive;
using Pororoca.Desktop.ExportImport;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class RequestAuthViewModel : ViewModelBase
{
    #region REQUEST AUTH

    [Reactive]
    public int AuthModeSelectedIndex { get; set; }

    [Reactive]
    public bool IsAuthModeNoneSelected { get; set; }

    [Reactive]
    public bool IsAuthModeBasicSelected { get; set; }

    [Reactive]
    public bool IsAuthModeBearerSelected { get; set; }

    [Reactive]
    public bool IsAuthModeClientCertificateSelected { get; set; }

    private PororocaRequestAuthMode? AuthMode =>
        AuthModeSelectedIndex switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            3 => PororocaRequestAuthMode.ClientCertificate,
            2 => PororocaRequestAuthMode.Bearer,
            1 => PororocaRequestAuthMode.Basic,
            _ => null
        };

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

    [Reactive]
    public bool IsClientCertificateTypeNoneSelected { get; set; }

    [Reactive]
    public bool IsClientCertificateTypePkcs12Selected { get; set; }

    [Reactive]
    public bool IsClientCertificateTypePemSelected { get; set; }

    private PororocaRequestAuthClientCertificateType? ClientCertificateType =>
        ClientCertificateTypeSelectedIndex switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            2 => PororocaRequestAuthClientCertificateType.Pem,
            1 => PororocaRequestAuthClientCertificateType.Pkcs12,
            _ => null
        };

    #region REQUEST AUTH CLIENT CERTIFICATE PKCS12

    [Reactive]
    public string? ClientCertificateAuthPkcs12CertificateFilePath { get; set; }

    [Reactive]
    public string? ClientCertificateAuthPkcs12FilePassword { get; set; }

    public ReactiveCommand<Unit, Unit> SearchClientCertificatePkcs12FileCmd { get; }

    #endregion

    #region REQUEST AUTH CLIENT CERTIFICATE PEM

    [Reactive]
    public string? ClientCertificateAuthPemCertificateFilePath { get; set; }

    [Reactive]
    public string? ClientCertificateAuthPemPrivateKeyFilePath { get; set; }

    [Reactive]
    public string? ClientCertificateAuthPemFilePassword { get; set; }

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