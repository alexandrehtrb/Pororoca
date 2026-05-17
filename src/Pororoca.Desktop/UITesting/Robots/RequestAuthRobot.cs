using Avalonia.Controls;
using Pororoca.Desktop.Controls;
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

    internal SyntaxHighlightingTextBox BasicAuthLogin => GetChildView<SyntaxHighlightingTextBox>("tbBasicAuthLogin")!;
    internal SyntaxHighlightingTextBox BasicAuthPassword => GetChildView<SyntaxHighlightingTextBox>("tbBasicAuthPassword")!;
    internal SyntaxHighlightingTextBox BearerAuthToken => GetChildView<SyntaxHighlightingTextBox>("tbBearerAuthToken")!;
    internal CheckBox WindowsAuthUseCurrentUser => GetChildView<CheckBox>("chkbWindowsAuthUseCurrentUser")!;
    internal SyntaxHighlightingTextBox WindowsAuthLogin => GetChildView<SyntaxHighlightingTextBox>("tbWindowsAuthLogin")!;
    internal SyntaxHighlightingTextBox WindowsAuthPassword => GetChildView<SyntaxHighlightingTextBox>("tbWindowsAuthPassword")!;
    internal SyntaxHighlightingTextBox WindowsAuthDomain => GetChildView<SyntaxHighlightingTextBox>("tbWindowsAuthDomain")!;
    internal SyntaxHighlightingTextBox ClientCertificatePkcs12FilePath => GetChildView<SyntaxHighlightingTextBox>("tbClientCertificatePkcs12FilePath")!;
    internal SyntaxHighlightingTextBox ClientCertificatePkcs12FilePassword => GetChildView<SyntaxHighlightingTextBox>("tbClientCertificatePkcs12FilePassword")!;
    internal SyntaxHighlightingTextBox ClientCertificatePemCertificateFilePath => GetChildView<SyntaxHighlightingTextBox>("tbClientCertificatePemCertificateFilePath")!;
    internal SyntaxHighlightingTextBox ClientCertificatePemPrivateKeyFilePath => GetChildView<SyntaxHighlightingTextBox>("tbClientCertificatePemPrivateKeyFilePath")!;
    internal SyntaxHighlightingTextBox ClientCertificatePemPrivateKeyPassword => GetChildView<SyntaxHighlightingTextBox>("tbClientCertificatePemPrivateKeyPassword")!;

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