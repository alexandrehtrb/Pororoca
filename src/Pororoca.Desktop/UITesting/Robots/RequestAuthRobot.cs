using Avalonia.Controls;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class RequestAuthRobot : BaseRobot
{
    public RequestAuthRobot(RequestAuthView rootView) : base(rootView) { }

    internal ComboBox AuthType => GetChildView<ComboBox>("cbReqAuthType")!;
    internal ComboBoxItem AuthTypeOptionNone => GetChildView<ComboBoxItem>("cbiReqAuthNone")!;
    internal ComboBoxItem AuthTypeOptionBasic => GetChildView<ComboBoxItem>("cbiReqAuthBasic")!;
    internal ComboBoxItem AuthTypeOptionBearer => GetChildView<ComboBoxItem>("cbiReqAuthBearer")!;
    internal ComboBoxItem AuthTypeOptionClientCertificate => GetChildView<ComboBoxItem>("cbiReqAuthClientCertificate")!;
    internal ComboBox ClientCertificateType => GetChildView<ComboBox>("cbReqAuthClientCertificateType")!;
    internal ComboBoxItem ClientCertificateTypeOptionNone => GetChildView<ComboBoxItem>("cbiReqAuthClientCertificateNone")!;
    internal ComboBoxItem ClientCertificateTypeOptionPkcs12 => GetChildView<ComboBoxItem>("cbiReqAuthClientCertificatePkcs12")!;
    internal ComboBoxItem ClientCertificateTypeOptionPem => GetChildView<ComboBoxItem>("cbiReqAuthClientCertificatePem")!;

    internal TextBox BasicAuthLogin => GetChildView<TextBox>("tbBasicAuthLogin")!;
    internal TextBox BasicAuthPassword => GetChildView<TextBox>("tbBasicAuthPassword")!;
    internal TextBox BearerAuthToken => GetChildView<TextBox>("tbBearerAuthToken")!;
    internal TextBox ClientCertificatePkcs12FilePath => GetChildView<TextBox>("tbClientCertificatePkcs12FilePath")!;
    internal TextBox ClientCertificatePkcs12FilePassword => GetChildView<TextBox>("tbClientCertificatePkcs12FilePassword")!;
    internal TextBox ClientCertificatePemCertificateFilePath => GetChildView<TextBox>("tbClientCertificatePemCertificateFilePath")!;
    internal TextBox ClientCertificatePemPrivateKeyFilePath => GetChildView<TextBox>("tbClientCertificatePemPrivateKeyFilePath")!;
    internal TextBox ClientCertificatePemPrivateKeyPassword => GetChildView<TextBox>("tbClientCertificatePemPrivateKeyPassword")!;
}