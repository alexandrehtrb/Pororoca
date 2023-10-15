using Avalonia.Controls;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class RequestAuthRobot : BaseRobot
{
    public RequestAuthRobot(RequestAuthView rootView) : base(rootView) { }

    internal ComboBox AuthType => GetChildView<ComboBox>("cbReqAuthType")!;
    internal ComboBoxItem AuthTypeOptionNone => GetChildView<ComboBoxItem>("cbiReqAuthNone")!;
    internal ComboBoxItem AuthTypeOptionInheritFromCollection => GetChildView<ComboBoxItem>("cbiReqAuthInheritFromCollection")!;
    internal ComboBoxItem AuthTypeOptionBasic => GetChildView<ComboBoxItem>("cbiReqAuthBasic")!;
    internal ComboBoxItem AuthTypeOptionBearer => GetChildView<ComboBoxItem>("cbiReqAuthBearer")!;
    internal ComboBoxItem AuthTypeOptionWindows => GetChildView<ComboBoxItem>("cbiReqAuthWindows")!;
    internal ComboBoxItem AuthTypeOptionClientCertificate => GetChildView<ComboBoxItem>("cbiReqAuthClientCertificate")!;
    internal ComboBox ClientCertificateType => GetChildView<ComboBox>("cbReqAuthClientCertificateType")!;
    internal ComboBoxItem ClientCertificateTypeOptionNone => GetChildView<ComboBoxItem>("cbiReqAuthClientCertificateNone")!;
    internal ComboBoxItem ClientCertificateTypeOptionPkcs12 => GetChildView<ComboBoxItem>("cbiReqAuthClientCertificatePkcs12")!;
    internal ComboBoxItem ClientCertificateTypeOptionPem => GetChildView<ComboBoxItem>("cbiReqAuthClientCertificatePem")!;

    internal TextBox BasicAuthLogin => GetChildView<TextBox>("tbBasicAuthLogin")!;
    internal TextBox BasicAuthPassword => GetChildView<TextBox>("tbBasicAuthPassword")!;
    internal TextBox BearerAuthToken => GetChildView<TextBox>("tbBearerAuthToken")!;
    internal CheckBox WindowsAuthUseCurrentUser => GetChildView<CheckBox>("chkbWindowsAuthUseCurrentUser")!;
    internal TextBox WindowsAuthLogin => GetChildView<TextBox>("tbWindowsAuthLogin")!;
    internal TextBox WindowsAuthPassword => GetChildView<TextBox>("tbWindowsAuthPassword")!;
    internal TextBox WindowsAuthDomain => GetChildView<TextBox>("tbWindowsAuthDomain")!;
    internal TextBox ClientCertificatePkcs12FilePath => GetChildView<TextBox>("tbClientCertificatePkcs12FilePath")!;
    internal TextBox ClientCertificatePkcs12FilePassword => GetChildView<TextBox>("tbClientCertificatePkcs12FilePassword")!;
    internal TextBox ClientCertificatePemCertificateFilePath => GetChildView<TextBox>("tbClientCertificatePemCertificateFilePath")!;
    internal TextBox ClientCertificatePemPrivateKeyFilePath => GetChildView<TextBox>("tbClientCertificatePemPrivateKeyFilePath")!;
    internal TextBox ClientCertificatePemPrivateKeyPassword => GetChildView<TextBox>("tbClientCertificatePemPrivateKeyPassword")!;

    internal async Task SetNoAuth() =>
        await AuthType.Select(AuthTypeOptionNone);

    internal async Task SetInheritFromCollectionAuth() =>
        await AuthType.Select(AuthTypeOptionInheritFromCollection);

    internal async Task SetBasicAuth(string login, string password)
    {
        await AuthType.Select(AuthTypeOptionBasic);
        await BasicAuthLogin.ClearAndTypeText(login);
        await BasicAuthPassword.ClearAndTypeText(password);
    }

    internal async Task SetBearerAuth(string token)
    {
        await AuthType.Select(AuthTypeOptionBearer);
        await BearerAuthToken.ClearAndTypeText(token);
    }

    internal async Task SetWindowsAuthCurrentUser()
    {
        await AuthType.Select(AuthTypeOptionWindows);
        if (WindowsAuthUseCurrentUser.IsChecked != true)
        {
            await WindowsAuthUseCurrentUser.ClickOn();
        }
    }

    internal async Task SetWindowsAuthOtherUser(string login, string password, string domain)
    {
        await AuthType.Select(AuthTypeOptionWindows);
        if (WindowsAuthUseCurrentUser.IsChecked == true)
        {
            await WindowsAuthUseCurrentUser.ClickOn();
        }
        await WindowsAuthLogin.ClearAndTypeText(login!);
        await WindowsAuthPassword.ClearAndTypeText(password!);
        await WindowsAuthDomain.ClearAndTypeText(domain!);
    }

    internal async Task SetPkcs12CertificateAuth(string certFilePath, string certPassword)
    {
        await AuthType.Select(AuthTypeOptionClientCertificate);
        await ClientCertificateType.Select(ClientCertificateTypeOptionPkcs12);
        await ClientCertificatePkcs12FilePath.ClearAndTypeText(certFilePath);
        await ClientCertificatePkcs12FilePassword.ClearAndTypeText(certPassword);
    }

    internal async Task SetPemCertificateAuth(string certFilePath, string prvKeyFilePath, string prvKeyPassword)
    {
        await AuthType.Select(AuthTypeOptionClientCertificate);
        await ClientCertificateType.Select(ClientCertificateTypeOptionPem);
        await ClientCertificatePemCertificateFilePath.ClearAndTypeText(certFilePath);
        await ClientCertificatePemPrivateKeyFilePath.ClearAndTypeText(prvKeyFilePath);
        await ClientCertificatePemPrivateKeyPassword.ClearAndTypeText(prvKeyPassword);
    }
}