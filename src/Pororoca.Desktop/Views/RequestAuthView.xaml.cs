using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Pororoca.Desktop.Views;

public class RequestAuthView : UserControl
{
    public RequestAuthView()
    {
        InitializeComponent();
        SetupSelectedOptionsPanelsVisibility();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void SetupSelectedOptionsPanelsVisibility()
    {
        ComboBoxItem cbiReqAuthNone = this.FindControl<ComboBoxItem>("cbiReqAuthNone")!,
            cbiReqAuthBasic = this.FindControl<ComboBoxItem>("cbiReqAuthBasic")!,
            cbiReqAuthBearer = this.FindControl<ComboBoxItem>("cbiReqAuthBearer")!,
            cbiReqAuthClientCertificate = this.FindControl<ComboBoxItem>("cbiReqAuthClientCertificate")!,
            cbiReqAuthClientCertificateNone = this.FindControl<ComboBoxItem>("cbiReqAuthClientCertificateNone")!,
            cbiReqAuthClientCertificatePkcs12 = this.FindControl<ComboBoxItem>("cbiReqAuthClientCertificatePkcs12")!,
            cbiReqAuthClientCertificatePem = this.FindControl<ComboBoxItem>("cbiReqAuthClientCertificatePem")!;

        StackPanel spReqAuthBasic = this.FindControl<StackPanel>("spReqAuthBasic")!,
            spReqAuthBearer = this.FindControl<StackPanel>("spReqAuthBearer")!,
            spReqAuthClientCertificatePkcs12 = this.FindControl<StackPanel>("spReqAuthClientCertificatePkcs12")!,
            spReqAuthClientCertificatePem = this.FindControl<StackPanel>("spReqAuthClientCertificatePem")!;

        var grReqAuthClientCertificate = this.FindControl<Grid>("grReqAuthClientCertificate")!;

        ComboBox cbReqAuthType = this.FindControl<ComboBox>("cbReqAuthType")!,
            cbReqAuthClientCertificateType = this.FindControl<ComboBox>("cbReqAuthClientCertificateType")!;

        cbReqAuthType.SelectionChanged += (sender, e) =>
        {
            object? selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (selected == cbiReqAuthNone)
            {
                spReqAuthBasic.IsVisible =
                spReqAuthBearer.IsVisible =
                grReqAuthClientCertificate.IsVisible =
                cbReqAuthClientCertificateType.IsVisible = false;
            }
            else if (selected == cbiReqAuthBasic)
            {
                spReqAuthBasic.IsVisible = true;
                spReqAuthBearer.IsVisible =
                grReqAuthClientCertificate.IsVisible =
                cbReqAuthClientCertificateType.IsVisible = false;
            }
            else if (selected == cbiReqAuthBearer)
            {
                spReqAuthBearer.IsVisible = true;
                spReqAuthBasic.IsVisible =
                grReqAuthClientCertificate.IsVisible =
                cbReqAuthClientCertificateType.IsVisible = false;
            }
            else if (selected == cbiReqAuthClientCertificate)
            {
                grReqAuthClientCertificate.IsVisible =
                cbReqAuthClientCertificateType.IsVisible = true;
                spReqAuthBasic.IsVisible =
                spReqAuthBearer.IsVisible = false;
            }
        };

        cbReqAuthClientCertificateType.SelectionChanged += (sender, e) =>
        {
            object? selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (selected == cbiReqAuthClientCertificateNone)
            {
                spReqAuthClientCertificatePkcs12.IsVisible =
                spReqAuthClientCertificatePem.IsVisible = false;
            }
            else if (selected == cbiReqAuthClientCertificatePkcs12)
            {
                spReqAuthClientCertificatePkcs12.IsVisible = true;
                spReqAuthClientCertificatePem.IsVisible = false;
            }
            else if (selected == cbiReqAuthClientCertificatePem)
            {
                spReqAuthClientCertificatePkcs12.IsVisible = false;
                spReqAuthClientCertificatePem.IsVisible = true;
            }
        };
    }
}