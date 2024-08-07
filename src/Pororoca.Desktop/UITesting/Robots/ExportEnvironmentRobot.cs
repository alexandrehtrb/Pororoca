using Avalonia.Controls;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class ExportEnvironmentRobot : BaseNamedRobot
{
    public ExportEnvironmentRobot(ExportEnvironmentView rootView)
     : base(rootView) { }

    internal Button GoBack => GetChildView<Button>("btGoBack")!;
    internal TextBlock EnvironmentName => GetChildView<TextBlock>("tbEnvironmentName")!;
    internal CheckBox IncludeSecretVariables => GetChildView<CheckBox>("cbIncludeSecretsWhenExporting")!;
    internal ComboBox Format => GetChildView<ComboBox>("cbExportFormat")!;
    internal ComboBoxItem FormatOptionPororoca => GetChildView<ComboBoxItem>("cbiExportFormatPororoca")!;
    internal ComboBoxItem FormatOptionPostman => GetChildView<ComboBoxItem>("cbiExportFormatPostman")!;

    internal async Task SetIncludeSecretVars(bool includeSecretVars)
    {
        IncludeSecretVariables.IsChecked = includeSecretVars;
        await UITestActions.WaitAfterActionAsync();
    }

    internal Task SelectExportFormat(ExportEnvironmentFormat format) =>
        Format.Select(format == ExportEnvironmentFormat.Postman ?
            FormatOptionPostman :
            FormatOptionPororoca);
}